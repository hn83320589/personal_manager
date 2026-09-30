using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Services;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Features.Files;

public sealed record FileDto(
    int Id, string FileName, string Url, FileKind Kind, string MimeType, long Size, int? Width, int? Height, DateTime CreatedAt);

public sealed class FileService(
    ApplicationDbContext db, ICurrentUser currentUser, IFileStorageProvider storage, IOptions<FileStorageSettings> settings)
{
    /// <summary>
    /// 單次請求的上限（含 multipart 額外資料），高於任何允許的單檔大小設定；
    /// 超過時伺服器在讀完內容前就拒絕，不會先把整個檔案寫進暫存。
    /// </summary>
    public const long RequestSizeCeiling = 60L * 1024 * 1024;

    public const int MaxPageSize = 100;

    public Task<PagedResult<FileDto>> GetMineAsync(FileKind? kind, int page, int pageSize)
    {
        var files = db.FileUploads.AsNoTracking().OwnedBy(currentUser.RequireUserId());
        if (kind is not null)
            files = files.Where(f => f.Kind == kind);
        return files
            .OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.Id)
            .Select(f => ToDto(f))
            .ToPagedResultAsync(page, pageSize, MaxPageSize);
    }

    public async Task<FileDto> UploadAsync(IFormFile? file)
    {
        var userId = currentUser.RequireUserId();
        if (file is null || file.Length == 0)
            throw new DomainValidationException("請選擇要上傳的檔案");

        var maxBytes = settings.Value.MaxFileSizeMB * 1024 * 1024;
        if (file.Length > maxBytes)
            throw new DomainValidationException($"檔案不能超過 {settings.Value.MaxFileSizeMB} MB");

        var inspection = FileInspector.Inspect(file.FileName, await ReadHeaderAsync(file));
        if (!inspection.IsAllowed)
            throw new DomainValidationException(inspection.Error!);

        await using var content = file.OpenReadStream();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var (url, storedName) = await storage.UploadAsync(content, extension, inspection.MimeType);

        var upload = new FileUpload
        {
            UserId = userId,
            FileName = DisplayName(file.FileName),
            StoredName = storedName,
            FileUrl = url,
            Kind = inspection.Kind,
            MimeType = inspection.MimeType,
            FileSize = file.Length,
            Width = inspection.Width,
            Height = inspection.Height
        };
        db.FileUploads.Add(upload);
        await db.SaveChangesAsync();
        return ToDto(upload);
    }

    public async Task DeleteAsync(int id)
    {
        var upload = await db.FileUploads.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(f => f.Id == id)
                     ?? throw new NotFoundException("找不到這個檔案");
        await storage.DeleteAsync(upload.StoredName);
        db.FileUploads.Remove(upload);
        await db.SaveChangesAsync();
    }

    private static async Task<byte[]> ReadHeaderAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var buffer = new byte[Math.Min(FileInspector.HeaderLength, file.Length)];
        await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false);
        return buffer;
    }

    /// <summary>只保留檔名本身（去除用戶端送來的路徑），並限制長度。</summary>
    private static string DisplayName(string fileName)
    {
        var name = Path.GetFileName(fileName.Replace('\\', '/'));
        return name.Length <= 255 ? name : name[..200] + Path.GetExtension(name);
    }

    private static FileDto ToDto(FileUpload f) =>
        new(f.Id, f.FileName, f.FileUrl, f.Kind, f.MimeType, f.FileSize, f.Width, f.Height, f.CreatedAt);
}
