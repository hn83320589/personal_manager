using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Mappings;

public static class MappingExtensions
{
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
