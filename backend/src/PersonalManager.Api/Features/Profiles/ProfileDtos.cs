using System.ComponentModel.DataAnnotations;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Profiles;

public sealed record SaveProfileRequest(
    [Required(ErrorMessage = "請輸入姓名"), StringLength(100)] string FullName,
    [StringLength(100)] string? Title,
    [StringLength(500)] string? Summary,
    [StringLength(10000)] string? Description,
    [StringLength(500)] string? ProfileImageUrl,
    [StringLength(200)] string? Website,
    [StringLength(100)] string? Location,
    [Required, RegularExpression("^(blue|green|purple|rose|slate)$", ErrorMessage = "不支援這個主題色")] string ThemeColor,
    [StringLength(100)] string? AvailabilityStatus,
    PortfolioMode PortfolioMode,
    CardStyle CardStyle,
    CardRatio CardRatio,
    SkillDisplay SkillDisplay);

/// <summary>公開頁面與後台共用：個人資料沒有需要對外隱藏的欄位（Email 放在 User，不在此）。</summary>
public sealed record ProfileDto(
    string Username, string FullName, string Title, string Summary, string Description, string ProfileImageUrl,
    string Website, string Location, string ThemeColor, string AvailabilityStatus,
    PortfolioMode PortfolioMode, CardStyle CardStyle, CardRatio CardRatio, SkillDisplay SkillDisplay);

/// <summary>使用者目錄的卡片。</summary>
public sealed record DirectoryCardDto(
    string Username, string FullName, string Title, string Summary, string ProfileImageUrl, string Location,
    string ThemeColor);
