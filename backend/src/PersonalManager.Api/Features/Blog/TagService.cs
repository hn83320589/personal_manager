using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Blog;

/// <summary>使用者自己的標籤（每人一份），文章與作品共用，供輸入時自動提示。</summary>
public sealed class TagService(ApplicationDbContext db, ICurrentUser currentUser)
{
    public Task<List<string>> GetMineAsync() =>
        db.Tags.AsNoTracking().Where(t => t.UserId == currentUser.RequireUserId())
            .OrderBy(t => t.Name).Select(t => t.Name).ToListAsync();

    /// <summary>
    /// 取得（必要時建立）對應的標籤實體。名稱去除前後空白、忽略大小寫去重，
    /// 保留使用者第一次輸入的寫法。
    /// </summary>
    public async Task<List<Tag>> ResolveAsync(int userId, IEnumerable<string>? names)
    {
        var wanted = (names ?? [])
            .Select(n => n?.Trim() ?? "")
            .Where(n => n.Length > 0)
            .DistinctBy(n => n.ToLowerInvariant())
            .ToList();
        if (wanted.Count == 0)
            return [];

        var lowered = wanted.Select(n => n.ToLower()).ToList();
        var existing = await db.Tags.Where(t => t.UserId == userId && lowered.Contains(t.Name.ToLower())).ToListAsync();

        var created = wanted
            .Where(n => !existing.Any(t => t.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .Select(n => new Tag { UserId = userId, Name = n })
            .ToList();
        db.Tags.AddRange(created);

        return [.. existing, .. created];
    }
}
