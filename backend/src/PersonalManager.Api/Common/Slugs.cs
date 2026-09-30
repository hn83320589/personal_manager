using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PersonalManager.Api.Common;

/// <summary>網址用的代稱：只允許小寫英數與連字號，例如 <c>offline-first-pwa</c>。</summary>
public static partial class Slugs
{
    public const string Pattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";
    private const int MaxLength = 80;

    public static bool IsValid(string slug) => ValidSlug().IsMatch(slug);

    /// <summary>由標題中的英數字組成；標題沒有英數字（例如全中文）時產生隨機代稱。</summary>
    public static string FromTitle(string title)
    {
        var slug = NonSlugCharacters().Replace(title.ToLowerInvariant(), "-").Trim('-');
        if (slug.Length > MaxLength)
            slug = slug[..MaxLength].TrimEnd('-');
        return slug.Length > 0 ? slug : "post-" + RandomToken(8);
    }

    /// <summary>若 <paramref name="baseSlug"/> 已被使用，依序嘗試 -2、-3…</summary>
    public static async Task<string> MakeUniqueAsync(string baseSlug, Func<string, Task<bool>> isTaken)
    {
        var candidate = baseSlug;
        for (var suffix = 2; await isTaken(candidate); suffix++)
            candidate = $"{baseSlug}-{suffix}";
        return candidate;
    }

    private static string RandomToken(int length)
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        return string.Create(length, 0, (chars, _) =>
        {
            for (var i = 0; i < chars.Length; i++)
                chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        });
    }

    [GeneratedRegex(Pattern)]
    private static partial Regex ValidSlug();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();
}
