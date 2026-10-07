using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Features.Tags;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Portfolios;

public sealed class PortfolioService(
    ApplicationDbContext db, ICurrentUser currentUser, TagService tags, PortfolioContentResolver content, TimeProvider clock)
{
    // ---------- public ----------

    public async Task<List<PortfolioCardDto>> GetPublicCardsAsync(string username, string? category, string? tag)
    {
        var query = PublicPortfolios(await db.RequirePublicUserIdAsync(username));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);
        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(p => p.Tags.Any(t => t.Name == tag));

        // 封面與區塊存成 JSON，無法在資料庫中投影成卡片；先取出需要的欄位再於記憶體中組合
        var rows = await query
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Id)
            .Select(p => new CardRow
            {
                Slug = p.Slug, Title = p.Title, Summary = p.Summary, Category = p.Category, Year = p.Year,
                Tags = p.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(), IsFeatured = p.IsFeatured,
                Covers = p.Covers, CoverFocus = p.CoverFocus, Blocks = p.Blocks
            })
            .ToListAsync();

        return rows.Select(r => new PortfolioCardDto(r.Slug, r.Title, r.Summary, r.Category, r.Year, r.Tags, r.IsFeatured,
                r.Covers, r.CoverFocus, r.Blocks.OfType<MetricsBlock>().FirstOrDefault()?.Items ?? []))
            .ToList();
    }

    public async Task<PublicPortfolioDto> GetPublicAsync(string username, string slug)
    {
        var p = await PublicPortfolios(await db.RequirePublicUserIdAsync(username))
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.Slug == slug)
                ?? throw new NotFoundException("找不到這件作品");
        return new PublicPortfolioDto(p.Slug, p.Title, p.Summary, p.Category, p.Year, p.Role, p.Period, TagNames(p),
            p.IsFeatured, p.Covers, p.CoverFocus, p.Fields, p.Links, p.Blocks);
    }

    public async Task<PortfolioFacetsDto> GetFacetsAsync(string username)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        var categories = await PublicPortfolios(userId)
            .Where(p => p.Category != "").Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
        var tagNames = await PublicPortfolios(userId)
            .SelectMany(p => p.Tags).Select(t => t.Name).Distinct().OrderBy(n => n).ToListAsync();
        return new PortfolioFacetsDto(categories, tagNames);
    }

    // ---------- me ----------

    public async Task<List<PortfolioSummaryDto>> GetMineAsync()
    {
        var rows = await db.Portfolios.AsNoTracking().OwnedBy(currentUser.RequireUserId())
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Id)
            .Select(p => new { p.Id, p.Title, p.Slug, p.Category, p.Year, p.IsFeatured, p.IsPublic, p.SortOrder, p.Covers, p.UpdatedAt })
            .ToListAsync();
        return rows.Select(p => new PortfolioSummaryDto(p.Id, p.Title, p.Slug, p.Category, p.Year, p.IsFeatured, p.IsPublic,
                p.SortOrder, p.Covers.FirstOrDefault()?.Url, p.UpdatedAt))
            .ToList();
    }

    public async Task<PortfolioDto> GetMineAsync(int id) => ToDto(await FindMineAsync(id));

    public async Task<PortfolioDto> CreateAsync(SavePortfolioRequest request)
    {
        var userId = currentUser.RequireUserId();
        var portfolio = new Portfolio { UserId = userId, SortOrder = await db.Portfolios.OwnedBy(userId).NextPositionAsync() };
        await ApplyAsync(portfolio, request);
        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();
        return ToDto(portfolio);
    }

    public async Task<PortfolioDto> UpdateAsync(int id, SavePortfolioRequest request)
    {
        var portfolio = await FindMineAsync(id);
        await ApplyAsync(portfolio, request);
        portfolio.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        return ToDto(portfolio);
    }

    public async Task DeleteAsync(int id)
    {
        db.Portfolios.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderRequest request)
    {
        await db.Portfolios.OwnedBy(currentUser.RequireUserId()).ApplyOrderAsync(request);
        await db.SaveChangesAsync();
    }

    // ---------- helpers ----------

    private IQueryable<Portfolio> PublicPortfolios(int userId) =>
        db.Portfolios.AsNoTracking().OwnedBy(userId).Where(p => p.IsPublic);

    private async Task<Portfolio> FindMineAsync(int id) =>
        await db.Portfolios.Include(p => p.Tags).OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NotFoundException("找不到這件作品");

    private async Task ApplyAsync(Portfolio portfolio, SavePortfolioRequest request)
    {
        var resolved = await content.ResolveAsync(portfolio.UserId, request);

        portfolio.Title = request.Title.Trim();
        portfolio.Summary = request.Summary?.Trim() ?? "";
        portfolio.Category = request.Category?.Trim() ?? "";
        portfolio.Year = request.Year;
        portfolio.Role = request.Role?.Trim() ?? "";
        portfolio.Period = request.Period?.Trim() ?? "";
        portfolio.IsFeatured = request.IsFeatured;
        portfolio.IsPublic = request.IsPublic;
        portfolio.CoverFocus = request.CoverFocus ?? Portfolio.DefaultCoverFocus;
        portfolio.Covers = resolved.Covers;
        portfolio.Fields = resolved.Fields;
        portfolio.Links = resolved.Links;
        portfolio.Blocks = resolved.Blocks;

        // 未指定網址時沿用原本的，改標題不會讓已分享出去的連結失效
        var baseSlug = !string.IsNullOrWhiteSpace(request.Slug) ? request.Slug
            : portfolio.Slug.Length > 0 ? portfolio.Slug
            : Slugs.FromTitle(portfolio.Title, "work");
        if (baseSlug != portfolio.Slug)
            portfolio.Slug = await Slugs.MakeUniqueAsync(baseSlug, candidate =>
                db.Portfolios.OwnedBy(portfolio.UserId).AnyAsync(p => p.Slug == candidate && p.Id != portfolio.Id));

        portfolio.Tags.Clear();
        foreach (var tag in await tags.ResolveAsync(portfolio.UserId, request.Tags))
            portfolio.Tags.Add(tag);
    }

    private static List<string> TagNames(Portfolio p) => p.Tags.Select(t => t.Name).OrderBy(n => n).ToList();

    private static PortfolioDto ToDto(Portfolio p) => new(
        p.Id, p.Title, p.Slug, p.Summary, p.Category, p.Year, p.Role, p.Period, TagNames(p), p.IsFeatured, p.IsPublic,
        p.SortOrder, p.Covers, p.CoverFocus, p.Fields, p.Links, p.Blocks, p.UpdatedAt);

    private sealed class CardRow
    {
        public string Slug { get; init; } = "";
        public string Title { get; init; } = "";
        public string Summary { get; init; } = "";
        public string Category { get; init; } = "";
        public int? Year { get; init; }
        public List<string> Tags { get; init; } = [];
        public bool IsFeatured { get; init; }
        public List<PortfolioImage> Covers { get; init; } = [];
        public string CoverFocus { get; init; } = "";
        public List<PortfolioBlock> Blocks { get; init; } = [];
    }
}
