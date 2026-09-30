using System.Text.Json.Serialization;

namespace PersonalManager.Api.Models;

// 作品內容（ADR-012）。這些值物件沒有各自的身分，永遠隨作品一起讀寫，以 JSON 存在作品資料列中。
// 圖片與附件的網址、尺寸、檔名等由伺服器依使用者上傳的檔案填入，不採信用戶端送來的值。

/// <summary>圖片：來自使用者上傳的檔案（<see cref="FileId"/>）或外部 https 網址。</summary>
public sealed record PortfolioImage(string Url, int? Width, int? Height, string Caption, string Alt, int? FileId);

/// <summary>自訂的作品資訊欄位，例如「客戶：山茶行」。</summary>
public sealed record PortfolioField(string Label, string Value);

public sealed record PortfolioLink(string Label, string Url);

/// <summary>附件：必須是使用者自己上傳的檔案。</summary>
public sealed record PortfolioFile(int FileId, string Url, string FileName, FileKind Kind, long Size, string Description);

public sealed record PortfolioMetric(string Value, string Label);

/// <summary>內容區塊。JSON 以 <c>type</c> 區分類型；未知的類型無法反序列化。</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextBlock), "text")]
[JsonDerivedType(typeof(ImageBlock), "image")]
[JsonDerivedType(typeof(GalleryBlock), "gallery")]
[JsonDerivedType(typeof(FilesBlock), "files")]
[JsonDerivedType(typeof(EmbedBlock), "embed")]
[JsonDerivedType(typeof(MetricsBlock), "metrics")]
[JsonDerivedType(typeof(CodeBlock), "code")]
public abstract record PortfolioBlock;

/// <summary><see cref="Html"/> 已經過 <see cref="Common.RichTextSanitizer"/> 清洗。</summary>
public sealed record TextBlock(string Title, string Html) : PortfolioBlock;

/// <summary><see cref="Layout"/>：narrow（與文字同寬）、wide（寬版）、bleed（滿版出血）。</summary>
public sealed record ImageBlock(string Layout, PortfolioImage Image) : PortfolioBlock;

/// <summary><see cref="Layout"/>：masonry（原比例）、cols-2、cols-3、stack（上下堆疊）。</summary>
public sealed record GalleryBlock(string Layout, IReadOnlyList<PortfolioImage> Items) : PortfolioBlock;

public sealed record FilesBlock(string Title, IReadOnlyList<PortfolioFile> Items) : PortfolioBlock;

/// <summary>只保存白名單網站的網址（<see cref="Common.EmbedProviders"/>），由前台轉成播放器。</summary>
public sealed record EmbedBlock(string Url, string Caption) : PortfolioBlock;

public sealed record MetricsBlock(string Title, IReadOnlyList<PortfolioMetric> Items) : PortfolioBlock;

public sealed record CodeBlock(string Language, string Code, string Caption) : PortfolioBlock;
