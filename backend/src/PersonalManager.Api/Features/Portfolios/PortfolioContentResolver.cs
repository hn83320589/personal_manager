using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Portfolios;

/// <summary>作品內容整理完成的結果，可直接寫入 <see cref="Portfolio"/>。</summary>
public sealed record PortfolioContent(
    List<PortfolioImage> Covers, List<PortfolioField> Fields, List<PortfolioLink> Links, List<PortfolioBlock> Blocks);

/// <summary>
/// 驗證作品內容並轉成儲存格式：檔案必須是使用者自己上傳的（圖片欄位只接受圖片），
/// 網址、尺寸、檔名由伺服器依檔案填入；文字區塊清洗 HTML；嵌入只接受白名單網站。
/// 任何一項不合法就丟 <see cref="DomainValidationException"/>，不做部分儲存。
/// </summary>
public sealed class PortfolioContentResolver(ApplicationDbContext db, RichTextSanitizer sanitizer)
{
    public const int MaxBlocks = 100;
    public const int MaxCovers = 10;
    public const int MaxFields = 20;
    public const int MaxLinks = 10;
    public const int MaxGalleryItems = 50;
    public const int MaxFileItems = 20;
    public const int MaxMetrics = 8;

    private static readonly string[] ImageLayouts = ["narrow", "wide", "bleed"];
    private static readonly string[] GalleryLayouts = ["masonry", "cols-2", "cols-3", "stack"];

    public async Task<PortfolioContent> ResolveAsync(int userId, SavePortfolioRequest request)
    {
        var covers = request.Covers ?? [];
        var fields = request.Fields ?? [];
        var links = request.Links ?? [];
        var blocks = request.Blocks ?? [];

        Require(covers.Count <= MaxCovers, $"封面最多 {MaxCovers} 張");
        Require(fields.Count <= MaxFields, $"作品資訊欄位最多 {MaxFields} 個");
        Require(links.Count <= MaxLinks, $"連結最多 {MaxLinks} 個");
        Require(blocks.Count <= MaxBlocks, $"內容區塊最多 {MaxBlocks} 個");

        var files = await LoadFilesAsync(userId, covers, blocks);

        return new PortfolioContent(
            covers.Select(c => ToImage(c, files)).ToList(),
            fields.Select(ToField).ToList(),
            links.Select(ToLink).ToList(),
            blocks.Select(b => ToBlock(b, files)).ToList());
    }

    /// <summary>一次載入內容中引用到的所有檔案；只查使用者自己的，別人的檔案視同不存在。</summary>
    private async Task<Dictionary<int, FileUpload>> LoadFilesAsync(
        int userId, IEnumerable<ImageInput> covers, IEnumerable<BlockInput> blocks)
    {
        var imageIds = covers.Concat(blocks.SelectMany(ImagesOf)).Select(i => i?.FileId);
        var attachmentIds = blocks.OfType<FilesBlockInput>().SelectMany(b => b.Items ?? []).Select(f => (int?)f?.FileId);
        var ids = imageIds.Concat(attachmentIds).OfType<int>().Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var files = await db.FileUploads.AsNoTracking().OwnedBy(userId).Where(f => ids.Contains(f.Id)).ToDictionaryAsync(f => f.Id);
        Require(files.Count == ids.Count, "找不到部分引用的檔案，請重新選擇");
        return files;
    }

    private static IEnumerable<ImageInput?> ImagesOf(BlockInput? block) => block switch
    {
        ImageBlockInput image => [image.Image],
        GalleryBlockInput gallery => [.. gallery.Items ?? []],
        _ => []
    };

    private PortfolioBlock ToBlock(BlockInput? block, Dictionary<int, FileUpload> files) => block switch
    {
        TextBlockInput text => new TextBlock(
            Text(text.Title, 200, "文字區塊標題"),
            sanitizer.Sanitize(Text(text.Html, 100_000, "文字區塊內容"))),

        ImageBlockInput image => new ImageBlock(
            OneOf(image.Layout, ImageLayouts, "圖片版型"),
            ToImage(image.Image ?? throw Invalid("圖片區塊需要一張圖片"), files)),

        GalleryBlockInput gallery => new GalleryBlock(
            OneOf(gallery.Layout, GalleryLayouts, "圖庫版型"),
            Limited(gallery.Items, MaxGalleryItems, "圖庫圖片").Select(i => ToImage(i, files)).ToList()),

        FilesBlockInput attachments => new FilesBlock(
            Text(attachments.Title, 200, "附件區塊標題"),
            Limited(attachments.Items, MaxFileItems, "附件").Select(f => ToAttachment(f, files)).ToList()),

        EmbedBlockInput embed => new EmbedBlock(
            EmbedProviders.IsAllowed(embed.Url) ? embed.Url!.Trim() : throw Invalid("不支援嵌入這個網站的內容"),
            Text(embed.Caption, 300, "嵌入說明")),

        MetricsBlockInput metrics => new MetricsBlock(
            Text(metrics.Title, 200, "數據區塊標題"),
            Limited(metrics.Items, MaxMetrics, "數據").Select(ToMetric).ToList()),

        CodeBlockInput code => new CodeBlock(
            Text(code.Language, 30, "程式語言"),
            Text(code.Code, 20_000, "程式碼"),
            Text(code.Caption, 300, "程式碼說明")),

        _ => throw Invalid("不支援的內容區塊類型")
    };

    private static PortfolioImage ToImage(ImageInput? input, Dictionary<int, FileUpload> files)
    {
        if (input is null)
            throw Invalid("圖片資料有誤");
        var caption = Text(input.Caption, 300, "圖片說明");
        var alt = Text(input.Alt, 300, "替代文字");

        if (input.FileId is { } fileId)
        {
            var file = files[fileId];
            Require(file.Kind == FileKind.Image, $"「{file.FileName}」不是圖片");
            return new PortfolioImage(file.FileUrl, file.Width, file.Height, caption, alt, file.Id);
        }

        Require(IsHttps(input.Url), "外部圖片需為 https 網址");
        return new PortfolioImage(input.Url!.Trim(), null, null, caption, alt, null);
    }

    private static PortfolioFile ToAttachment(FileInput? input, Dictionary<int, FileUpload> files)
    {
        if (input is null)
            throw Invalid("附件資料有誤");
        var file = files[input.FileId];
        return new PortfolioFile(file.Id, file.FileUrl, file.FileName, file.Kind, file.FileSize,
            Text(input.Description, 300, "附件說明"));
    }

    private static PortfolioField ToField(PortfolioField? field) =>
        field is null
            ? throw Invalid("作品資訊欄位有誤")
            : new PortfolioField(Required(field.Label, 50, "欄位名稱"), Text(field.Value, 200, "欄位內容"));

    private static PortfolioLink ToLink(PortfolioLink? link)
    {
        if (link is null)
            throw Invalid("連結資料有誤");
        Require(IsHttp(link.Url), "連結需為 http 或 https 網址");
        return new PortfolioLink(Required(link.Label, 50, "連結名稱"), link.Url!.Trim());
    }

    private static PortfolioMetric ToMetric(PortfolioMetric? metric) =>
        metric is null
            ? throw Invalid("數據資料有誤")
            : new PortfolioMetric(Required(metric.Value, 30, "數據數值"), Text(metric.Label, 100, "數據說明"));

    // ---------- 驗證小工具 ----------

    private static string Text(string? value, int maxLength, string name)
    {
        var trimmed = value?.Trim() ?? "";
        Require(trimmed.Length <= maxLength, $"{name}最多 {maxLength} 字");
        return trimmed;
    }

    private static string Required(string? value, int maxLength, string name)
    {
        var text = Text(value, maxLength, name);
        Require(text.Length > 0, $"請輸入{name}");
        return text;
    }

    /// <summary>未指定時使用清單中的第一個值。</summary>
    private static string OneOf(string? value, string[] allowed, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            return allowed[0];
        Require(allowed.Contains(value), $"{name}「{value}」不存在");
        return value;
    }

    private static IReadOnlyList<T?> Limited<T>(IReadOnlyList<T?>? items, int max, string name)
    {
        items ??= [];
        Require(items.Count <= max, $"{name}最多 {max} 個");
        return items;
    }

    private static bool IsHttps(string? url) =>
        Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

    private static bool IsHttp(string? url) =>
        Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw Invalid(message);
    }

    private static DomainValidationException Invalid(string message) => new(message);
}
