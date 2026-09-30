using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

/// <summary>
/// 一件作品（ADR-012）：基本資訊與固定欄位（角色、期間）為一般欄位，
/// 封面、自訂欄位、連結與內容區塊以 JSON 儲存，整份作品一起讀寫。
/// </summary>
public class Portfolio : IOwnedByUser, ISortable
{
    public const string DefaultCoverFocus = "50% 50%";

    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>網址代稱，在同一位使用者的作品中唯一。</summary>
    [StringLength(80)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public int? Year { get; set; }

    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    [StringLength(100)]
    public string Period { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }

    /// <summary>卡片比例與圖片不同時保留的位置，CSS object-position 格式，例如「50% 30%」。</summary>
    [StringLength(20)]
    public string CoverFocus { get; set; } = DefaultCoverFocus;

    public List<PortfolioImage> Covers { get; set; } = [];
    public List<PortfolioField> Fields { get; set; } = [];
    public List<PortfolioLink> Links { get; set; } = [];
    public List<PortfolioBlock> Blocks { get; set; } = [];

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
