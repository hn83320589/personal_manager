using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Guestbook;

/// <summary>每位使用者有自己的留言板；留言需經版主（被留言的使用者）審核才會公開。</summary>
public sealed class GuestbookService(ApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public const int MaxPageSize = 50;

    public async Task<PagedResult<PublicGuestbookEntryDto>> GetApprovedAsync(string username, int page, int pageSize)
    {
        var ownerId = await db.RequirePublicUserIdAsync(username);
        return await db.GuestBookEntries.AsNoTracking()
            .Where(e => e.TargetUserId == ownerId && e.IsApproved)
            .OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id)
            .Select(e => new PublicGuestbookEntryDto(e.Id, e.Name, e.Message, e.AdminReply, e.CreatedAt, e.RepliedAt))
            .ToPagedResultAsync(page, pageSize, MaxPageSize);
    }

    public async Task LeaveAsync(string username, LeaveMessageRequest request)
    {
        var ownerId = await db.RequirePublicUserIdAsync(username);
        db.GuestBookEntries.Add(new GuestBookEntry
        {
            TargetUserId = ownerId,
            Name = request.Name.Trim(),
            Email = request.Email?.Trim() ?? "",
            Message = request.Message.Trim(),
            IsApproved = false
        });
        await db.SaveChangesAsync();
    }

    public Task<PagedResult<GuestbookEntryDto>> GetMineAsync(GuestbookFilter filter, int page, int pageSize)
    {
        var query = MyEntries().AsNoTracking();
        query = filter switch
        {
            GuestbookFilter.Pending => query.Where(e => !e.IsApproved),
            GuestbookFilter.Approved => query.Where(e => e.IsApproved),
            _ => query
        };
        return query
            .OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id)
            .Select(e => new GuestbookEntryDto(e.Id, e.Name, e.Email, e.Message, e.IsApproved, e.AdminReply, e.CreatedAt, e.RepliedAt))
            .ToPagedResultAsync(page, pageSize, MaxPageSize);
    }

    public async Task SetApprovalAsync(int id, bool isApproved)
    {
        var entry = await FindMineAsync(id);
        entry.IsApproved = isApproved;
        entry.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
    }

    public async Task ReplyAsync(int id, string? reply)
    {
        var entry = await FindMineAsync(id);
        var now = clock.GetUtcNow().UtcDateTime;
        entry.AdminReply = reply?.Trim() ?? "";
        entry.RepliedAt = entry.AdminReply.Length > 0 ? now : null;
        entry.UpdatedAt = now;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        db.GuestBookEntries.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    /// <summary>留言的擁有者是「被留言的人」（TargetUserId），不是留言者。</summary>
    private IQueryable<GuestBookEntry> MyEntries()
    {
        var me = currentUser.RequireUserId();
        return db.GuestBookEntries.Where(e => e.TargetUserId == me);
    }

    private async Task<GuestBookEntry> FindMineAsync(int id) =>
        await MyEntries().FirstOrDefaultAsync(e => e.Id == id) ?? throw new NotFoundException("找不到這則留言");
}
