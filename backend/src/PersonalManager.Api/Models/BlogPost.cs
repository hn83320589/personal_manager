using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

/// <summary>
/// 文章狀態。「排程」不是獨立狀態：<see cref="Published"/> 且 <see cref="BlogPost.PublishedAt"/> 在未來即為排程，
/// 時間到了自動出現在公開頁面。
/// </summary>
public enum BlogPostStatus
{
    Draft,
    Published,
    Archived
}

public class BlogPost : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>網址代稱，在同一位使用者的文章中唯一。</summary>
    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>已清洗過的 HTML（<see cref="Common.RichTextSanitizer"/>）。</summary>
    public string Content { get; set; } = string.Empty;

    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [StringLength(500)]
    public string CoverImageUrl { get; set; } = string.Empty;

    /// <summary>存檔時計算，列表頁不需讀取全文。</summary>
    public int ReadingMinutes { get; set; } = 1;

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
