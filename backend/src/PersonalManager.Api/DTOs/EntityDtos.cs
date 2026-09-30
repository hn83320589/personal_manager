using System.ComponentModel.DataAnnotations;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.DTOs;

// ===== Portfolio =====
public class CreatePortfolioDto
{
    public int UserId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }
}
public class UpdatePortfolioDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ProjectUrl { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? Technologies { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsPublic { get; set; }
    public int? SortOrder { get; set; }
}
public class PortfolioResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Technologies { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== PortfolioAttachment =====
public class CreatePortfolioAttachmentDto
{
    public int PortfolioId { get; set; }
    public int? FileUploadId { get; set; }
    [Required] public string FileName { get; set; } = string.Empty;
    [Required] public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }
}
public class UpdatePortfolioAttachmentDto
{
    public int? SortOrder { get; set; }
}
public class PortfolioAttachmentResponse
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public int? FileUploadId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
