using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Mappings;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Services;

// ===== User Service =====
public interface IUserService : ICrudService<User, CreateUserDto, UpdateUserDto, UserResponse> { }

public class UserService : CrudService<User, CreateUserDto, UpdateUserDto, UserResponse>, IUserService
{
    public UserService(IRepository<User> repo) : base(repo) { }
    protected override User MapToEntity(CreateUserDto dto)
    {
        var entity = dto.ToEntity();
        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        return entity;
    }
    protected override UserResponse MapToResponse(User entity) => entity.ToResponse();
    protected override void ApplyUpdate(User entity, UpdateUserDto dto) => entity.ApplyUpdate(dto);
}

// ===== Portfolio Service =====
public interface IPortfolioService : ICrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>
{
    Task<List<PortfolioResponse>> GetByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetFeaturedAsync(int userId);
}

public class PortfolioService : CrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>, IPortfolioService
{
    public PortfolioService(IRepository<Portfolio> repo) : base(repo) { }
    protected override Portfolio MapToEntity(CreatePortfolioDto dto) => dto.ToEntity();
    protected override PortfolioResponse MapToResponse(Portfolio entity) => entity.ToResponse();
    protected override void ApplyUpdate(Portfolio entity, UpdatePortfolioDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetFeaturedAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsFeatured && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== FileUpload Service =====
public interface IFileUploadService
{
    Task<List<FileUploadResponse>> GetByUserIdAsync(int userId);
    Task<FileUploadResponse> UploadAsync(IFormFile file, int userId);
    Task<bool> DeleteAsync(int id, int userId);
}

public class FileUploadService : IFileUploadService
{
    private readonly IRepository<FileUpload> _repo;
    private readonly IFileStorageProvider _storage;

    public FileUploadService(IRepository<FileUpload> repo, IFileStorageProvider storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<List<FileUploadResponse>> GetByUserIdAsync(int userId)
    {
        var items = await _repo.FindAsync(f => f.UserId == userId);
        return items.OrderByDescending(f => f.CreatedAt).Select(f => f.ToResponse()).ToList();
    }

    public async Task<FileUploadResponse> UploadAsync(IFormFile file, int userId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var (fileUrl, storedName) = await _storage.UploadAsync(file, ext);

        var fileType = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
            ".pdf" => "pdf",
            ".doc" or ".docx" => "document",
            ".ppt" or ".pptx" => "presentation",
            _ => "other"
        };

        var entity = new FileUpload
        {
            UserId = userId,
            FileName = file.FileName,
            StoredName = storedName,
            FileUrl = fileUrl,
            FileType = fileType,
            FileSize = file.Length,
            MimeType = file.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repo.AddAsync(entity);
        return saved.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var items = await _repo.FindAsync(f => f.Id == id && f.UserId == userId);
        var entity = items.FirstOrDefault();
        if (entity == null) return false;

        await _storage.DeleteAsync(entity.StoredName);
        return await _repo.DeleteAsync(id);
    }
}

// ===== PortfolioAttachment Service =====
public interface IPortfolioAttachmentService : ICrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>
{
    Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId);
}

public class PortfolioAttachmentService : CrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>, IPortfolioAttachmentService
{
    public PortfolioAttachmentService(IRepository<PortfolioAttachment> repo) : base(repo) { }
    protected override PortfolioAttachment MapToEntity(CreatePortfolioAttachmentDto dto) => dto.ToEntity();
    protected override PortfolioAttachmentResponse MapToResponse(PortfolioAttachment entity) => entity.ToResponse();
    protected override void ApplyUpdate(PortfolioAttachment entity, UpdatePortfolioAttachmentDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId)
    {
        var items = await Repository.FindAsync(a => a.PortfolioId == portfolioId);
        return items.OrderBy(a => a.SortOrder).Select(MapToResponse).ToList();
    }
}
