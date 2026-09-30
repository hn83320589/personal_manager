using System.ComponentModel.DataAnnotations;
using PersonalManager.Api.Common;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Blog;

/// <summary>
/// 新增與更新共用。<see cref="Slug"/> 留空時由標題產生；<see cref="PublishedAt"/> 在未來即為排程，
/// 發佈但未指定時間時以現在時間發佈。
/// </summary>
public sealed record SavePostRequest(
    [Required(ErrorMessage = "請輸入標題"), StringLength(200)] string Title,
    [StringLength(80), RegularExpression(Slugs.Pattern, ErrorMessage = "網址只能使用小寫英文、數字與連字號")] string? Slug,
    [StringLength(500_000)] string? Content,
    [StringLength(500)] string? Summary,
    [StringLength(50)] string? Category,
    [MaxLength(10, ErrorMessage = "標籤最多 10 個")] IReadOnlyList<string>? Tags,
    [StringLength(500)] string? CoverImageUrl,
    BlogPostStatus Status,
    DateTime? PublishedAt) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Tags?.Any(t => t?.Trim().Length > 50) == true)
            yield return new ValidationResult("每個標籤最多 50 字", [nameof(Tags)]);
        if (!IsSafeImageUrl(CoverImageUrl))
            yield return new ValidationResult("封面圖片需為 http(s) 網址或上傳的檔案", [nameof(CoverImageUrl)]);
    }

    private static bool IsSafeImageUrl(string? url) =>
        string.IsNullOrWhiteSpace(url)
        || url.StartsWith("/files/", StringComparison.Ordinal)
        || (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp));
}

/// <summary>公開列表：不含全文。</summary>
public sealed record PublicPostSummaryDto(
    string Slug, string Title, string Summary, string Category, IReadOnlyList<string> Tags, string CoverImageUrl,
    DateTime PublishedAt, int ReadingMinutes);

public sealed record PublicPostDto(
    string Slug, string Title, string Summary, string Category, IReadOnlyList<string> Tags, string CoverImageUrl,
    DateTime PublishedAt, int ReadingMinutes, string Content);

public sealed record PostFacetsDto(IReadOnlyList<string> Categories, IReadOnlyList<string> Tags);

/// <summary>後台列表：不含全文。</summary>
public sealed record MyPostSummaryDto(
    int Id, string Title, string Slug, BlogPostStatus Status, DateTime? PublishedAt, DateTime UpdatedAt,
    string Category, IReadOnlyList<string> Tags, string CoverImageUrl, int ViewCount);

public sealed record MyPostDto(
    int Id, string Title, string Slug, string Content, string Summary, string Category, IReadOnlyList<string> Tags,
    string CoverImageUrl, BlogPostStatus Status, DateTime? PublishedAt, DateTime UpdatedAt, int ViewCount,
    int ReadingMinutes);
