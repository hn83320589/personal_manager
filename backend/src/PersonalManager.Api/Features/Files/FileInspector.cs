using System.Buffers.Binary;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Files;

public sealed record FileInspection(bool IsAllowed, string? Error, FileKind Kind, string MimeType, int? Width, int? Height)
{
    public static FileInspection Reject(string error) => new(false, error, default, "", null, null);
}

/// <summary>
/// 依副檔名與檔案開頭的簽章（magic bytes）判斷檔案類型。兩者必須一致才接受，
/// 避免把 HTML 等可執行內容改副檔名後上傳。MIME 類型由這裡決定，不採用用戶端送來的值。
/// 設計原始檔（AI、PSD）與 SVG（可夾帶 script）不接受（ADR-012）。
/// </summary>
public static class FileInspector
{
    /// <summary>判斷格式與讀取圖片尺寸所需的檔頭長度。</summary>
    public const int HeaderLength = 64 * 1024;

    private enum Signature { Png, Jpeg, Gif, WebP, Pdf, Zip, OleCompound }

    private sealed record FileType(FileKind Kind, string MimeType, Signature Signature);

    private static readonly Dictionary<string, FileType> ByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new(FileKind.Image, "image/jpeg", Signature.Jpeg),
        [".jpeg"] = new(FileKind.Image, "image/jpeg", Signature.Jpeg),
        [".png"] = new(FileKind.Image, "image/png", Signature.Png),
        [".gif"] = new(FileKind.Image, "image/gif", Signature.Gif),
        [".webp"] = new(FileKind.Image, "image/webp", Signature.WebP),
        [".pdf"] = new(FileKind.Pdf, "application/pdf", Signature.Pdf),
        [".docx"] = new(FileKind.Word, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Signature.Zip),
        [".doc"] = new(FileKind.Word, "application/msword", Signature.OleCompound),
        [".pptx"] = new(FileKind.PowerPoint, "application/vnd.openxmlformats-officedocument.presentationml.presentation", Signature.Zip),
        [".ppt"] = new(FileKind.PowerPoint, "application/vnd.ms-powerpoint", Signature.OleCompound),
        [".xlsx"] = new(FileKind.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Signature.Zip),
        [".xls"] = new(FileKind.Excel, "application/vnd.ms-excel", Signature.OleCompound),
        [".zip"] = new(FileKind.Archive, "application/zip", Signature.Zip),
    };

    public static IReadOnlyCollection<string> AllowedExtensions => ByExtension.Keys;

    public static FileInspection Inspect(string fileName, ReadOnlySpan<byte> header)
    {
        var extension = Path.GetExtension(fileName);
        if (!ByExtension.TryGetValue(extension, out var type))
            return FileInspection.Reject("不支援這種檔案格式。圖片請用 JPG、PNG、WebP、GIF；文件請用 PDF、Word、PowerPoint、Excel 或 ZIP");
        if (!HasSignature(header, type.Signature))
            return FileInspection.Reject("檔案內容與副檔名不符");

        var (width, height) = type.Kind == FileKind.Image ? ReadImageSize(header, type.Signature) : (null, null);
        return new FileInspection(true, null, type.Kind, type.MimeType, width, height);
    }

    private static bool HasSignature(ReadOnlySpan<byte> h, Signature signature) => signature switch
    {
        Signature.Png => h.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        Signature.Jpeg => h.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]),
        Signature.Gif => h.StartsWith("GIF87a"u8) || h.StartsWith("GIF89a"u8),
        Signature.WebP => h.Length >= 12 && h.StartsWith("RIFF"u8) && h[8..12].SequenceEqual("WEBP"u8),
        Signature.Pdf => h.StartsWith("%PDF-"u8),
        Signature.Zip => h.StartsWith((ReadOnlySpan<byte>)[0x50, 0x4B, 0x03, 0x04]) || h.StartsWith((ReadOnlySpan<byte>)[0x50, 0x4B, 0x05, 0x06]),
        Signature.OleCompound => h.StartsWith((ReadOnlySpan<byte>)[0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]),
        _ => false
    };

    /// <summary>只讀檔頭取得寬高，讓前台在圖片載入前就能保留正確比例的版面。讀不到時回傳 null。</summary>
    private static (int?, int?) ReadImageSize(ReadOnlySpan<byte> h, Signature signature)
    {
        try
        {
            return signature switch
            {
                Signature.Png when h.Length >= 24 =>
                    (BinaryPrimitives.ReadInt32BigEndian(h[16..]), BinaryPrimitives.ReadInt32BigEndian(h[20..])),
                Signature.Gif when h.Length >= 10 =>
                    (BinaryPrimitives.ReadUInt16LittleEndian(h[6..]), BinaryPrimitives.ReadUInt16LittleEndian(h[8..])),
                Signature.Jpeg => ReadJpegSize(h),
                Signature.WebP => ReadWebPSize(h),
                _ => (null, null)
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return (null, null);   // 檔頭不完整：仍接受檔案，只是沒有尺寸
        }
    }

    /// <summary>逐段略過 JPEG 區段，直到遇到記錄尺寸的 SOF（Start of Frame）區段。</summary>
    private static (int?, int?) ReadJpegSize(ReadOnlySpan<byte> h)
    {
        var i = 2;
        while (i + 9 < h.Length)
        {
            if (h[i] != 0xFF) return (null, null);
            var marker = h[i + 1];
            var isStartOfFrame = marker is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC;
            if (isStartOfFrame)
                return (BinaryPrimitives.ReadUInt16BigEndian(h[(i + 7)..]), BinaryPrimitives.ReadUInt16BigEndian(h[(i + 5)..]));
            i += 2 + BinaryPrimitives.ReadUInt16BigEndian(h[(i + 2)..]);
        }
        return (null, null);
    }

    private static (int?, int?) ReadWebPSize(ReadOnlySpan<byte> h)
    {
        var chunk = h[12..16];
        if (chunk.SequenceEqual("VP8X"u8))
            return (ReadUInt24(h[24..]) + 1, ReadUInt24(h[27..]) + 1);
        if (chunk.SequenceEqual("VP8L"u8))
        {
            var bits = BinaryPrimitives.ReadUInt32LittleEndian(h[21..]);
            return ((int)(bits & 0x3FFF) + 1, (int)((bits >> 14) & 0x3FFF) + 1);
        }
        if (chunk.SequenceEqual("VP8 "u8))
            return (BinaryPrimitives.ReadUInt16LittleEndian(h[26..]) & 0x3FFF, BinaryPrimitives.ReadUInt16LittleEndian(h[28..]) & 0x3FFF);
        return (null, null);
    }

    private static int ReadUInt24(ReadOnlySpan<byte> b) => b[0] | (b[1] << 8) | (b[2] << 16);
}
