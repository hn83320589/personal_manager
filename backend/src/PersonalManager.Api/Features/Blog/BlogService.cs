using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Blog;

public sealed class BlogService(
    ApplicationDbContext db, ICurrentUser currentUser, TagService tags, RichTextSanitizer sanitizer, TimeProvider clock)
{
    public const int MaxPageSize = 50;

    // ---------- public ----------

    public async Task<PagedResult<PublicPostSummaryDto>> GetPublishedAsync(
        string username, string? keyword, string? tag, string? category, int page, int pageSize)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        var query = Published(userId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(p => p.Title.Contains(k) || p.Summary.Contains(k));
        }
        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(p => p.Tags.Any(t => t.Name == tag));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        return await query
            .OrderByDescending(p => p.PublishedAt).ThenByDescending(p => p.Id)
            .Select(p => new PublicPostSummaryDto(p.Slug, p.Title, p.Summary, p.Category,
                p.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(), p.CoverImageUrl, p.PublishedAt!.Value, p.ReadingMinutes))
            .ToPagedResultAsync(page, pageSize, MaxPageSize);
    }

    public async Task<PublicPostDto> GetPublishedPostAsync(string username, string slug)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        return await Published(userId)
                   .Where(p => p.Slug == slug)
                   .Select(p => new PublicPostDto(p.Slug, p.Title, p.Summary, p.Category,
                       p.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(), p.CoverImageUrl, p.PublishedAt!.Value,
                       p.ReadingMinutes, p.Content))
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("找不到這篇文章");
    }

    public async Task<PostFacetsDto> GetFacetsAsync(string username)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        var categories = await Published(userId)
            .Where(p => p.Category != "").Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
        var tagNames = await Published(userId)
            .SelectMany(p => p.Tags).Select(t => t.Name).Distinct().OrderBy(n => n).ToListAsync();
        return new PostFacetsDto(categories, tagNames);
    }

    /// <summary>以單一 UPDATE 遞增，同時多人瀏覽也不會漏算。</summary>
    public async Task RecordViewAsync(string username, string slug)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        var updated = await Published(userId).Where(p => p.Slug == slug)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1));
        if (updated == 0)
            throw new NotFoundException("找不到這篇文章");
    }

    // ---------- me ----------

    public Task<PagedResult<MyPostSummaryDto>> GetMineAsync(string? keyword, BlogPostStatus? status, int page, int pageSize)
    {
        var query = db.BlogPosts.AsNoTracking().OwnedBy(currentUser.RequireUserId());
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.Title.Contains(keyword.Trim()));
        if (status is not null)
            query = query.Where(p => p.Status == status);

        return query
            .OrderByDescending(p => p.UpdatedAt).ThenByDescending(p => p.Id)
            .Select(p => new MyPostSummaryDto(p.Id, p.Title, p.Slug, p.Status, p.PublishedAt, p.UpdatedAt, p.Category,
                p.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(), p.CoverImageUrl, p.ViewCount))
            .ToPagedResultAsync(page, pageSize, MaxPageSize);
    }

    public async Task<MyPostDto> GetMyPostAsync(int id) => ToDto(await FindMineAsync(id));

    public async Task<MyPostDto> CreateAsync(SavePostRequest request)
    {
        var post = new BlogPost { UserId = currentUser.RequireUserId() };
        await ApplyAsync(post, request);
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        return ToDto(post);
    }

    public async Task<MyPostDto> UpdateAsync(int id, SavePostRequest request)
    {
        var post = await FindMineAsync(id);
        await ApplyAsync(post, request);
        post.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        return ToDto(post);
    }

    public async Task DeleteAsync(int id)
    {
        db.BlogPosts.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    // ---------- helpers ----------

    /// <summary>公開可見：已發佈且發佈時間已到（未來的發佈時間即為排程）。</summary>
    private IQueryable<BlogPost> Published(int userId)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return db.BlogPosts.AsNoTracking().OwnedBy(userId)
            .Where(p => p.Status == BlogPostStatus.Published && p.PublishedAt != null && p.PublishedAt <= now);
    }

    private async Task<BlogPost> FindMineAsync(int id) =>
        await db.BlogPosts.Include(p => p.Tags).OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NotFoundException("找不到這篇文章");

    private async Task ApplyAsync(BlogPost post, SavePostRequest request)
    {
        post.Title = request.Title.Trim();
        post.Content = sanitizer.Sanitize(request.Content);
        post.ReadingMinutes = RichTextSanitizer.EstimateReadingMinutes(post.Content);
        post.Summary = request.Summary?.Trim() ?? "";
        post.Category = request.Category?.Trim() ?? "";
        post.CoverImageUrl = request.CoverImageUrl?.Trim() ?? "";
        post.Status = request.Status;
        post.PublishedAt = request.Status == BlogPostStatus.Published
            ? request.PublishedAt?.ToUniversalTime() ?? post.PublishedAt ?? clock.GetUtcNow().UtcDateTime
            : request.PublishedAt?.ToUniversalTime() ?? post.PublishedAt;

        var baseSlug = string.IsNullOrWhiteSpace(request.Slug) ? Slugs.FromTitle(post.Title) : request.Slug;
        if (baseSlug != post.Slug)
            post.Slug = await Slugs.MakeUniqueAsync(baseSlug, candidate =>
                db.BlogPosts.OwnedBy(post.UserId).AnyAsync(p => p.Slug == candidate && p.Id != post.Id));

        post.Tags.Clear();
        foreach (var tag in await tags.ResolveAsync(post.UserId, request.Tags))
            post.Tags.Add(tag);
    }

    private static MyPostDto ToDto(BlogPost p) => new(
        p.Id, p.Title, p.Slug, p.Content, p.Summary, p.Category, p.Tags.Select(t => t.Name).OrderBy(n => n).ToList(),
        p.CoverImageUrl, p.Status, p.PublishedAt, p.UpdatedAt, p.ViewCount, p.ReadingMinutes);
}
