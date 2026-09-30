using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.WorkTracking;

public sealed class WorkTaskService(ApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<List<WorkTaskDto>> GetMineAsync(int? projectId, WorkTaskStatus? status)
    {
        var userId = currentUser.RequireUserId();
        var tasks = db.WorkTasks.AsNoTracking().OwnedBy(userId);
        if (projectId is not null)
            tasks = tasks.Where(t => t.ProjectId == projectId);
        if (status is not null)
            tasks = tasks.Where(t => t.Status == status);

        return await Rows(tasks, userId)
            .OrderBy(r => r.Task.Status == WorkTaskStatus.Completed || r.Task.Status == WorkTaskStatus.Cancelled)
            .ThenBy(r => r.Task.DueDate == null).ThenBy(r => r.Task.DueDate)
            .ThenByDescending(r => r.Task.Id)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<WorkTaskDto> CreateAsync(SaveWorkTaskRequest request)
    {
        var userId = currentUser.RequireUserId();
        var task = new WorkTask { UserId = userId };
        await ApplyAsync(task, request);
        db.WorkTasks.Add(task);
        await db.SaveChangesAsync();
        return await LoadAsync(task.Id, userId);
    }

    public async Task<WorkTaskDto> UpdateAsync(int id, SaveWorkTaskRequest request)
    {
        var task = await FindMineAsync(id);
        await ApplyAsync(task, request);
        task.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        return await LoadAsync(task.Id, task.UserId);
    }

    public async Task DeleteAsync(int id)
    {
        db.WorkTasks.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 專案名稱由關聯取得，實際時數為時間紀錄加總，都在同一個查詢完成。
    /// 先投影成 <see cref="TaskRow"/> 以便排序（EF 無法對 record 建構子投影後的結果排序），最後才轉成 DTO。
    /// </summary>
    private IQueryable<TaskRow> Rows(IQueryable<WorkTask> tasks, int userId) =>
        from t in tasks
        join p in db.Projects on t.ProjectId equals p.Id into projects
        from project in projects.DefaultIfEmpty()
        select new TaskRow
        {
            Task = t,
            ProjectName = project != null ? project.Name : null,
            ActualMinutes = db.TimeEntries.Where(e => e.UserId == userId && e.WorkTaskId == t.Id).Sum(e => (int?)e.DurationMinutes) ?? 0
        };

    private async Task<WorkTaskDto> LoadAsync(int id, int userId) =>
        ToDto(await Rows(db.WorkTasks.AsNoTracking().Where(t => t.Id == id), userId).FirstAsync());

    private static WorkTaskDto ToDto(TaskRow r) => new(
        r.Task.Id, r.Task.Title, r.Task.Description, r.Task.ProjectId, r.ProjectName, r.Task.Priority, r.Task.Status,
        r.Task.EstimatedHours, r.ActualMinutes, r.Task.DueDate, r.Task.CompletedAt);

    private sealed class TaskRow
    {
        public required WorkTask Task { get; init; }
        public string? ProjectName { get; init; }
        public int ActualMinutes { get; init; }
    }

    private async Task<WorkTask> FindMineAsync(int id) =>
        await db.WorkTasks.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(t => t.Id == id)
        ?? throw new NotFoundException("找不到這個任務");

    private async Task ApplyAsync(WorkTask task, SaveWorkTaskRequest request)
    {
        if (request.ProjectId is { } projectId
            && !await db.Projects.OwnedBy(task.UserId).AnyAsync(p => p.Id == projectId))
            throw new DomainValidationException("找不到指定的專案");

        task.Title = request.Title.Trim();
        task.Description = request.Description ?? "";
        task.ProjectId = request.ProjectId;
        task.Priority = request.Priority;
        task.EstimatedHours = request.EstimatedHours;
        task.DueDate = request.DueDate?.ToUniversalTime();
        task.CompletedAt = request.Status == WorkTaskStatus.Completed
            ? task.CompletedAt ?? clock.GetUtcNow().UtcDateTime
            : null;
        task.Status = request.Status;
    }
}
