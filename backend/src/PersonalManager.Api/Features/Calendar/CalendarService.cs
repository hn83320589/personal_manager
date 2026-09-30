using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Calendar;

public sealed class CalendarService(ApplicationDbContext db, ICurrentUser currentUser)
{
    /// <summary>一次查詢的最長區間，避免一次展開、回傳過多資料。</summary>
    public static readonly TimeSpan MaxRange = TimeSpan.FromDays(366);

    public async Task<List<OccurrenceDto>> GetPublicOccurrencesAsync(string username, DateTime? from, DateTime? to)
    {
        var range = ValidateRange(from, to);
        var userId = await db.RequirePublicUserIdAsync(username);
        return await OccurrencesAsync(db.CalendarEvents.OwnedBy(userId).Where(e => e.IsPublic), range);
    }

    public Task<List<OccurrenceDto>> GetMyOccurrencesAsync(DateTime? from, DateTime? to)
    {
        var range = ValidateRange(from, to);
        return OccurrencesAsync(db.CalendarEvents.OwnedBy(currentUser.RequireUserId()), range);
    }

    public async Task<EventDto> GetMyEventAsync(int id) => ToDto(await FindMineAsync(id));

    public async Task<EventDto> CreateAsync(SaveEventRequest request)
    {
        var calendarEvent = new CalendarEvent { UserId = currentUser.RequireUserId() };
        Apply(calendarEvent, request);
        db.CalendarEvents.Add(calendarEvent);
        await db.SaveChangesAsync();
        return ToDto(calendarEvent);
    }

    public async Task<EventDto> UpdateAsync(int id, SaveEventRequest request)
    {
        var calendarEvent = await FindMineAsync(id);
        Apply(calendarEvent, request);
        calendarEvent.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToDto(calendarEvent);
    }

    public async Task DeleteAsync(int id)
    {
        db.CalendarEvents.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    private static (DateTime From, DateTime To) ValidateRange(DateTime? from, DateTime? to)
    {
        if (from is null || to is null)
            throw new DomainValidationException("請指定查詢的起訖時間（from、to）");
        var (start, end) = (from.Value.ToUniversalTime(), to.Value.ToUniversalTime());
        if (end <= start)
            throw new DomainValidationException("結束時間必須晚於開始時間");
        if (end - start > MaxRange)
            throw new DomainValidationException("查詢區間最長一年");
        return (start, end);
    }

    /// <summary>
    /// 先在資料庫篩出可能落在區間內的事件（一般事件看起訖、重複事件看開始與重複結束日），
    /// 再於記憶體中展開重複規則。
    /// </summary>
    private static async Task<List<OccurrenceDto>> OccurrencesAsync(IQueryable<CalendarEvent> events, (DateTime From, DateTime To) range)
    {
        var fromDate = DateOnly.FromDateTime(range.From);
        var candidates = await events.AsNoTracking()
            .Where(e => e.StartTime < range.To)
            .Where(e => e.Recurrence == Recurrence.None
                ? e.EndTime >= range.From
                : e.RecurrenceUntil == null || e.RecurrenceUntil >= fromDate)
            .ToListAsync();

        return candidates
            .SelectMany(e => RecurrenceExpander
                .Expand(e.StartTime, e.EndTime, e.Recurrence, e.RecurrenceUntil, range.From, range.To)
                .Select(o => new OccurrenceDto(e.Id, e.Title, e.Description, o.Start, o.End, e.IsAllDay, e.Color,
                    e.Recurrence != Recurrence.None)))
            .OrderBy(o => o.Start).ThenBy(o => o.EventId)
            .ToList();
    }

    private async Task<CalendarEvent> FindMineAsync(int id) =>
        await db.CalendarEvents.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(e => e.Id == id)
        ?? throw new NotFoundException("找不到這個行程");

    private static void Apply(CalendarEvent e, SaveEventRequest r)
    {
        e.Title = r.Title.Trim();
        e.Description = r.Description ?? "";
        e.StartTime = r.StartTime.ToUniversalTime();
        e.EndTime = r.EndTime.ToUniversalTime();
        e.IsAllDay = r.IsAllDay;
        e.IsPublic = r.IsPublic;
        e.Color = r.Color ?? "";
        e.Recurrence = r.Recurrence;
        e.RecurrenceUntil = r.Recurrence == Recurrence.None ? null : r.RecurrenceUntil;
    }

    private static EventDto ToDto(CalendarEvent e) => new(
        e.Id, e.Title, e.Description, e.StartTime, e.EndTime, e.IsAllDay, e.IsPublic, e.Color, e.Recurrence, e.RecurrenceUntil);
}
