using System.Buffers.Binary;
using System.Text;

namespace PersonalManager.Tests.Infrastructure;

/// <summary>測試用的最小檔案內容：只包含辨識格式與讀取尺寸所需的檔頭。</summary>
public static class SampleFiles
{
    public static byte[] Png(int width, int height)
    {
        var bytes = new byte[33];
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.CopyTo(bytes, 0);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(8), 13);
        Encoding.ASCII.GetBytes("IHDR").CopyTo(bytes, 12);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16), width);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20), height);
        return bytes;
    }

    public static byte[] Jpeg(int width, int height)
    {
        var app0 = new byte[] { 0xFF, 0xE0, 0x00, 0x10, (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 };
        var sof0 = new byte[19];
        new byte[] { 0xFF, 0xC0, 0x00, 0x11, 0x08 }.CopyTo(sof0, 0);
        BinaryPrimitives.WriteUInt16BigEndian(sof0.AsSpan(5), (ushort)height);
        BinaryPrimitives.WriteUInt16BigEndian(sof0.AsSpan(7), (ushort)width);
        return [0xFF, 0xD8, .. app0, .. sof0, 0xFF, 0xD9];
    }

    public static byte[] Gif(int width, int height)
    {
        var bytes = new byte[13];
        Encoding.ASCII.GetBytes("GIF89a").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(6), (ushort)width);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8), (ushort)height);
        return bytes;
    }

    /// <summary>延伸格式（VP8X）的 WebP，尺寸以 24-bit 的「寬 - 1」「高 - 1」儲存。</summary>
    public static byte[] WebPExtended(int width, int height)
    {
        var bytes = new byte[30];
        Encoding.ASCII.GetBytes("RIFF").CopyTo(bytes, 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), 22);
        Encoding.ASCII.GetBytes("WEBPVP8X").CopyTo(bytes, 8);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(16), 10);
        WriteUInt24(bytes, 24, width - 1);
        WriteUInt24(bytes, 27, height - 1);
        return bytes;
    }

    /// <summary>無損格式（VP8L）的 WebP，尺寸以 14 bits 的「寬 - 1」「高 - 1」打包。</summary>
    public static byte[] WebPLossless(int width, int height)
    {
        var bytes = new byte[25];
        Encoding.ASCII.GetBytes("RIFF").CopyTo(bytes, 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), 17);
        Encoding.ASCII.GetBytes("WEBPVP8L").CopyTo(bytes, 8);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(16), 5);
        bytes[20] = 0x2F;
        var packed = (uint)(width - 1) | ((uint)(height - 1) << 14);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(21), packed);
        return bytes;
    }

    public static byte[] Pdf() => Encoding.ASCII.GetBytes("%PDF-1.7\n%âãÏÓ\n1 0 obj\n<<>>\nendobj\n%%EOF");

    /// <summary>docx、pptx、xlsx 與 zip 都是 ZIP 容器。</summary>
    public static byte[] Zip() => [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, .. new byte[20]];

    /// <summary>舊版 Office（doc、ppt、xls）的 OLE 容器。</summary>
    public static byte[] OleCompound() => [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, .. new byte[24]];

    public static byte[] Html() => Encoding.UTF8.GetBytes("<html><script>alert(document.cookie)</script></html>");

    private static void WriteUInt24(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
    }
}
