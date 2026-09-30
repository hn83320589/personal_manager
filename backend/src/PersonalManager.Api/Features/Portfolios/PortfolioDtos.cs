using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PersonalManager.Api.Common;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Portfolios;

// ---------- 輸入 ----------

/// <summary>
/// 新增與更新共用；更新時整份作品一起取代。只有標題必填，其餘欄位省略即為空白。
/// 圖片與附件只需提供 fileId（或外部圖片網址），網址、尺寸、檔名由伺服器填入。
/// </summary>
public sealed record SavePortfolioRequest(
    [Required(ErrorMessage = "請輸入作品名稱"), StringLength(200)] string Title,
    [StringLength(80), RegularExpression(Slugs.Pattern, ErrorMessage = "網址只能使用小寫英文、數字與連字號")] string? Slug = null,
    [StringLength(500)] string? Summary = null,
    [StringLength(50)] string? Category = null,
    [Range(1900, 2100, ErrorMessage = "年份需介於 1900 到 2100")] int? Year = null,
    [StringLength(100)] string? Role = null,
    [StringLength(100)] string? Period = null,
    [MaxLength(10, ErrorMessage = "標籤最多 10 個")] IReadOnlyList<string>? Tags = null,
    bool IsFeatured = false,
    bool IsPublic = true,
    [RegularExpression(@"^\d{1,3}% \d{1,3}%$", ErrorMessage = "封面位置格式應為「50% 50%」")] string? CoverFocus = null,
    IReadOnlyList<ImageInput>? Covers = null,
    IReadOnlyList<PortfolioField>? Fields = null,
    IReadOnlyList<PortfolioLink>? Links = null,
    IReadOnlyList<BlockInput>? Blocks = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Tags?.Any(t => t?.Trim().Length > 50) == true)
            yield return new ValidationResult("每個標籤最多 50 字", [nameof(Tags)]);
    }
}

/// <summary>使用者上傳的圖片（<see cref="FileId"/>）或外部 https 圖片網址，兩者擇一。</summary>
public sealed record ImageInput(int? FileId, string? Url, string? Caption, string? Alt);

public sealed record FileInput(int FileId, string? Description);

/// <summary>
/// 區塊輸入，以 <c>type</c> 區分。基底類別不是 abstract：缺少 type 時會得到基底類別，
/// 由 <see cref="PortfolioContentResolver"/> 回 400，而不是反序列化失敗成 500。
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextBlockInput), "text")]
[JsonDerivedType(typeof(ImageBlockInput), "image")]
[JsonDerivedType(typeof(GalleryBlockInput), "gallery")]
[JsonDerivedType(typeof(FilesBlockInput), "files")]
[JsonDerivedType(typeof(EmbedBlockInput), "embed")]
[JsonDerivedType(typeof(MetricsBlockInput), "metrics")]
[JsonDerivedType(typeof(CodeBlockInput), "code")]
public record BlockInput;

public sealed record TextBlockInput(string? Title, string? Html) : BlockInput;
public sealed record ImageBlockInput(string? Layout, ImageInput? Image) : BlockInput;
public sealed record GalleryBlockInput(string? Layout, IReadOnlyList<ImageInput>? Items) : BlockInput;
public sealed record FilesBlockInput(string? Title, IReadOnlyList<FileInput>? Items) : BlockInput;
public sealed record EmbedBlockInput(string? Url, string? Caption) : BlockInput;
public sealed record MetricsBlockInput(string? Title, IReadOnlyList<PortfolioMetric>? Items) : BlockInput;
public sealed record CodeBlockInput(string? Language, string? Code, string? Caption) : BlockInput;

// ---------- 輸出 ----------

/// <summary>公開列表的卡片：封面（輪播）與第一個數據區塊的重點數字，不含內容區塊。</summary>
public sealed record PortfolioCardDto(
    string Slug, string Title, string Summary, string Category, int? Year, IReadOnlyList<string> Tags, bool IsFeatured,
    IReadOnlyList<PortfolioImage> Covers, string CoverFocus, IReadOnlyList<PortfolioMetric> Metrics);

public sealed record PublicPortfolioDto(
    string Slug, string Title, string Summary, string Category, int? Year, string Role, string Period,
    IReadOnlyList<string> Tags, bool IsFeatured, IReadOnlyList<PortfolioImage> Covers, string CoverFocus,
    IReadOnlyList<PortfolioField> Fields, IReadOnlyList<PortfolioLink> Links, IReadOnlyList<PortfolioBlock> Blocks);

public sealed record PortfolioFacetsDto(IReadOnlyList<string> Categories, IReadOnlyList<string> Tags);

/// <summary>後台列表：只含管理需要的欄位與第一張封面。</summary>
public sealed record PortfolioSummaryDto(
    int Id, string Title, string Slug, string Category, int? Year, bool IsFeatured, bool IsPublic, int SortOrder,
    string? CoverUrl, DateTime UpdatedAt);

public sealed record PortfolioDto(
    int Id, string Title, string Slug, string Summary, string Category, int? Year, string Role, string Period,
    IReadOnlyList<string> Tags, bool IsFeatured, bool IsPublic, int SortOrder, IReadOnlyList<PortfolioImage> Covers,
    string CoverFocus, IReadOnlyList<PortfolioField> Fields, IReadOnlyList<PortfolioLink> Links,
    IReadOnlyList<PortfolioBlock> Blocks, DateTime UpdatedAt);
