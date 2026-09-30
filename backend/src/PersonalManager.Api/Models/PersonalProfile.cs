using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class PersonalProfile : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string ProfileImageUrl { get; set; } = string.Empty;

    [StringLength(200)]
    public string Website { get; set; } = string.Empty;

    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [StringLength(50)]
    public string ThemeColor { get; set; } = "blue";

    /// <summary>目前狀態，例如「接受品牌與出版委託」。</summary>
    [StringLength(100)]
    public string AvailabilityStatus { get; set; } = string.Empty;

    public PortfolioMode PortfolioMode { get; set; } = PortfolioMode.Designer;
    public CardStyle CardStyle { get; set; } = CardStyle.Visual;
    public CardRatio CardRatio { get; set; } = CardRatio.Portrait;
    public SkillDisplay SkillDisplay { get; set; } = SkillDisplay.NameOnly;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
