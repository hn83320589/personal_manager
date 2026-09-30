using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Ganss.Xss;

namespace PersonalManager.Api.Common;

/// <summary>
/// 富文本（文章內容、作品文字區塊）存入前的清洗。只保留編輯器會產生的標籤與屬性，
/// 其餘一律移除，前台就能直接顯示而不必擔心 XSS。前台顯示前仍會以 DOMPurify 再清洗一次。
/// </summary>
public sealed partial class RichTextSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public RichTextSanitizer()
    {
        _sanitizer = new HtmlSanitizer();

        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.UnionWith([
            "p", "br", "strong", "b", "em", "i", "u", "s", "a", "h2", "h3", "h4", "blockquote", "ul", "ol", "li",
            "pre", "code", "hr", "figure", "figcaption", "img", "div"
        ]);

        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.UnionWith(["href", "src", "alt", "title", "class", "style", "target", "data-embed"]);
        _sanitizer.UriAttributes.Add("data-embed");

        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedCssProperties.Add("text-align");
        _sanitizer.AllowedAtRules.Clear();

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);

        _sanitizer.PostProcessDom += (_, e) => ApplyEditorRules(e.Document);
    }

    public string Sanitize(string? html) => string.IsNullOrWhiteSpace(html) ? "" : _sanitizer.Sanitize(html);

    /// <summary>
    /// 預估閱讀時間：中日韓文字以每分鐘 400 字、其他語言以每分鐘 200 個單字計算，最少 1 分鐘。
    /// </summary>
    public static int EstimateReadingMinutes(string sanitizedHtml)
    {
        var text = HtmlTag().Replace(sanitizedHtml, " ");
        var cjkCharacters = CjkCharacter().Count(text);
        var otherWords = LatinWord().Count(CjkCharacter().Replace(text, " "));
        return Math.Max(1, (int)Math.Ceiling(cjkCharacters / 400.0 + otherWords / 200.0));
    }

    private static void ApplyEditorRules(IDocument document)
    {
        // div 只用來承載嵌入內容，且網址必須在白名單內
        foreach (var div in document.QuerySelectorAll("div").ToList())
        {
            if (!EmbedProviders.IsAllowed(div.GetAttribute("data-embed")))
                div.Remove();
            else
                foreach (var attribute in div.Attributes.Where(a => a.Name != "data-embed").ToList())
                    div.RemoveAttribute(attribute.Name);
        }

        // class 只保留程式碼區塊的語言標記（語法上色用）
        foreach (var element in document.QuerySelectorAll("[class]").ToList())
        {
            var keep = element.TagName == "CODE" && LanguageClass().IsMatch(element.GetAttribute("class") ?? "");
            if (!keep)
                element.RemoveAttribute("class");
        }

        // data-embed 只能出現在 div 上
        foreach (var element in document.QuerySelectorAll("[data-embed]").Where(e => e.TagName != "DIV").ToList())
            element.RemoveAttribute("data-embed");

        // 外部連結另開視窗時不得讓目標頁面取得 window.opener
        foreach (var link in document.QuerySelectorAll("a"))
        {
            if (link.GetAttribute("target") is { } target && target != "_blank")
                link.RemoveAttribute("target");
            link.SetAttribute("rel", "noopener noreferrer nofollow");
        }
    }

    [GeneratedRegex("^language-[a-z0-9+#-]{1,30}$")]
    private static partial Regex LanguageClass();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTag();

    [GeneratedRegex(@"[\p{IsCJKUnifiedIdeographs}\p{IsHiragana}\p{IsKatakana}\p{IsHangulSyllables}]")]
    private static partial Regex CjkCharacter();

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex LatinWord();
}
