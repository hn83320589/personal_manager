using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.WorkTracking;

public sealed class TimeEntryService(ApplicationDbContext db, ICurrentUser currentUser)
{
    public async Task<List<TimeEntryDto>> GetMineAsync(DateOnly? from, DateOnly? to, int? workTaskId)
    {
        var entries = Filter(currentUser.RequireUserId(), from, to);
        if (workTaskId is not null)
            entries = entries.Where(e => e.WorkTaskId == workTaskId);

        return await Rows(entries)
            .OrderByDescending(r => r.Entry.Date).ThenByDescending(r => r.Entry.StartTime).ThenByDescending(r => r.Entry.Id)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<TimeSummaryDto> GetSummaryAsync(DateOnly? from, DateOnly? to)
    {
        var entries = (await Rows(Filter(currentUser.RequireUserId(), from, to)).ToListAsync()).Select(ToDto).ToList();
        var projectIds = await ProjectIdsByTaskAsync(entries);

        var byProject = entries
            .GroupBy(e => (Id: e.WorkTaskId is { } taskId ? projectIds.GetValueOrDefault(taskId) : null, Name: e.ProjectName))
            .Select(g => new ProjectTotalDto(g.Key.Id, g.Key.Name, g.Sum(e => e.DurationMinutes)))
            .OrderByDescending(p => p.Minutes)
            .ToList();
        var byDay = entries
            .GroupBy(e => e.Date)
            .Select(g => new DailyTotalDto(g.Key, g.Sum(e => e.DurationMinutes)))
            .OrderBy(d => d.Date)
            .ToList();

        return new TimeSummaryDto(entries.Sum(e => e.DurationMinutes), byProject, byDay);
    }

    public async Task<TimeEntryDto> CreateAsync(SaveTimeEntryRequest request)
    {
        var entry = new TimeEntry { UserId = currentUser.RequireUserId() };
        await ApplyAsync(entry, request);
        db.TimeEntries.Add(entry);
        await db.SaveChangesAsync();
        return await LoadAsync(entry.Id);
    }

    public async Task<TimeEntryDto> UpdateAsync(int id, SaveTimeEntryRequest request)
    {
        var entry = await FindMineAsync(id);
        await ApplyAsync(entry, request);
        entry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return await LoadAsync(entry.Id);
    }

    public async Task DeleteAsync(int id)
    {
        db.TimeEntries.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    private IQueryable<TimeEntry> Filter(int userId, DateOnly? from, DateOnly? to)
    {
        var entries = db.TimeEntries.AsNoTracking().OwnedBy(userId);
        if (from is not null)
            entries = entries.Where(e => e.Date >= from);
        if (to is not null)
            entries = entries.Where(e => e.Date <= to);
        return entries;
    }

    /// <summary>任務標題與專案名稱由關聯取得；先投影成 <see cref="EntryRow"/> 以便排序，最後才轉成 DTO。</summary>
    private IQueryable<EntryRow> Rows(IQueryable<TimeEntry> entries) =>
        from e in entries
        join t in db.WorkTasks on e.WorkTaskId equals t.Id into tasks
        from task in tasks.DefaultIfEmpty()
        join p in db.Projects on task.ProjectId equals p.Id into projects
        from project in projects.DefaultIfEmpty()
        select new EntryRow
        {
            Entry = e,
            WorkTaskTitle = task != null ? task.Title : null,
            ProjectName = project != null ? project.Name : null
        };

    private static TimeEntryDto ToDto(EntryRow r) => new(
        r.Entry.Id, r.Entry.WorkTaskId, r.WorkTaskTitle, r.ProjectName, r.Entry.Title, r.Entry.Date,
        r.Entry.StartTime, r.Entry.EndTime, r.Entry.DurationMinutes, r.Entry.Description);

    private sealed class EntryRow
    {
        public required TimeEntry Entry { get; init; }
        public string? WorkTaskTitle { get; init; }
        public string? ProjectName { get; init; }
    }

    private async Task<Dictionary<int, int?>> ProjectIdsByTaskAsync(IEnumerable<TimeEntryDto> entries)
    {
        var taskIds = entries.Where(e => e.WorkTaskId != null).Select(e => e.WorkTaskId!.Value).Distinct().ToList();
        return await db.WorkTasks.AsNoTracking().Where(t => taskIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.ProjectId);
    }

    private async Task<TimeEntryDto> LoadAsync(int id) =>
        ToDto(await Rows(db.TimeEntries.AsNoTracking().Where(e => e.Id == id)).FirstAsync());

    private async Task<TimeEntry> FindMineAsync(int id) =>
        await db.TimeEntries.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(e => e.Id == id)
        ?? throw new NotFoundException("找不到這筆時間紀錄");

    private async Task ApplyAsync(TimeEntry entry, SaveTimeEntryRequest request)
    {
        if (request.WorkTaskId is { } taskId && !await db.WorkTasks.OwnedBy(entry.UserId).AnyAsync(t => t.Id == taskId))
            throw new DomainValidationException("找不到指定的任務");

        entry.WorkTaskId = request.WorkTaskId;
        entry.Title = request.Title?.Trim() ?? "";
        entry.Date = request.Date;
        entry.StartTime = request.StartTime;
        entry.EndTime = request.EndTime;
        entry.DurationMinutes = request.ResolveDurationMinutes();
        entry.Description = request.Description ?? "";
    }
}
