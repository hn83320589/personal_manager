namespace PersonalManager.Api.Common;

/// <summary>
/// 允許嵌入的外部網站（ADR-012）。內容中只保存網址，前台依同一份白名單轉成播放器，
/// 資料庫裡永遠不會有 &lt;iframe&gt;。清單需與前端保持一致。
/// </summary>
public static class EmbedProviders
{
    // 值為 true 時接受子網域（例如 www.youtube.com、player.vimeo.com）
    private static readonly Dictionary<string, bool> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["youtube.com"] = true,
        ["youtu.be"] = false,
        ["vimeo.com"] = true,
        ["figma.com"] = true,
        ["sketchfab.com"] = true,
        ["soundcloud.com"] = true,
        ["codepen.io"] = true,
        ["docs.google.com"] = false,
        ["gist.github.com"] = false
    };

    public static bool IsAllowed(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var host = uri.Host;
        return AllowedHosts.Any(entry =>
            host.Equals(entry.Key, StringComparison.OrdinalIgnoreCase)
            || (entry.Value && host.EndsWith("." + entry.Key, StringComparison.OrdinalIgnoreCase)));
    }
}
