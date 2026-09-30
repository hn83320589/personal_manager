using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Mappings;

public static class MappingExtensions
{
    // ===== User =====
    public static UserResponse ToResponse(this User u) => new()
    {
        Id = u.Id, Username = u.Username, Email = u.Email,
        FullName = u.FullName, Role = u.Role, IsActive = u.IsActive, CreatedAt = u.CreatedAt
    };
    public static User ToEntity(this CreateUserDto d) => new()
    {
        Username = d.Username, Email = d.Email, FullName = d.FullName, Role = d.Role
    };
    public static void ApplyUpdate(this User u, UpdateUserDto d)
    {
        if (d.Email != null) u.Email = d.Email;
        if (d.FullName != null) u.FullName = d.FullName;
        if (d.Role != null) u.Role = d.Role;
        if (d.IsActive.HasValue) u.IsActive = d.IsActive.Value;
    }

    // ===== Portfolio =====
    public static PortfolioResponse ToResponse(this Portfolio p) => new()
    {
        Id = p.Id, UserId = p.UserId, Title = p.Title, Description = p.Description,
        ImageUrl = p.ImageUrl, ProjectUrl = p.ProjectUrl, RepositoryUrl = p.RepositoryUrl,
        Technologies = p.Technologies, IsFeatured = p.IsFeatured, IsPublic = p.IsPublic,
        SortOrder = p.SortOrder, CreatedAt = p.CreatedAt
    };
    public static Portfolio ToEntity(this CreatePortfolioDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        ImageUrl = d.ImageUrl, ProjectUrl = d.ProjectUrl, RepositoryUrl = d.RepositoryUrl,
        Technologies = d.Technologies, IsFeatured = d.IsFeatured,
        IsPublic = d.IsPublic, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this Portfolio p, UpdatePortfolioDto d)
    {
        if (d.Title != null) p.Title = d.Title;
        if (d.Description != null) p.Description = d.Description;
        if (d.ImageUrl != null) p.ImageUrl = d.ImageUrl;
        if (d.ProjectUrl != null) p.ProjectUrl = d.ProjectUrl;
        if (d.RepositoryUrl != null) p.RepositoryUrl = d.RepositoryUrl;
        if (d.Technologies != null) p.Technologies = d.Technologies;
        if (d.IsFeatured.HasValue) p.IsFeatured = d.IsFeatured.Value;
        if (d.IsPublic.HasValue) p.IsPublic = d.IsPublic.Value;
        if (d.SortOrder.HasValue) p.SortOrder = d.SortOrder.Value;
    }

    // ===== FileUpload =====
    public static FileUploadResponse ToResponse(this FileUpload f) => new()
    {
        Id = f.Id, UserId = f.UserId, FileName = f.FileName, StoredName = f.StoredName,
        FileUrl = f.FileUrl, FileType = f.FileType, FileSize = f.FileSize,
        MimeType = f.MimeType, CreatedAt = f.CreatedAt
    };

    // ===== PortfolioAttachment =====
    public static PortfolioAttachmentResponse ToResponse(this PortfolioAttachment a) => new()
    {
        Id = a.Id, PortfolioId = a.PortfolioId, FileUploadId = a.FileUploadId,
        FileName = a.FileName, FileUrl = a.FileUrl, FileType = a.FileType,
        FileSize = a.FileSize, SortOrder = a.SortOrder, CreatedAt = a.CreatedAt
    };
    public static PortfolioAttachment ToEntity(this CreatePortfolioAttachmentDto d) => new()
    {
        PortfolioId = d.PortfolioId, FileUploadId = d.FileUploadId, FileName = d.FileName,
        FileUrl = d.FileUrl, FileType = d.FileType, FileSize = d.FileSize, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this PortfolioAttachment a, UpdatePortfolioAttachmentDto d)
    {
        if (d.SortOrder.HasValue) a.SortOrder = d.SortOrder.Value;
    }

}
