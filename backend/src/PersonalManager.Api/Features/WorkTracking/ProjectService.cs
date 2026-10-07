using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.WorkTracking;

/// <summary>工作追蹤用的專案（與作品集無關）。刪除專案時其下任務保留、改為未分類。</summary>
public sealed class ProjectService(ApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public Task<List<ProjectDto>> GetMineAsync() =>
        db.Projects.AsNoTracking().OwnedBy(currentUser.RequireUserId())
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Id)
            .Select(p => ToDto(p))
            .ToListAsync();

    public async Task<ProjectDto> CreateAsync(SaveProjectRequest request)
    {
        var userId = currentUser.RequireUserId();
        var project = new Project { UserId = userId, SortOrder = await db.Projects.OwnedBy(userId).NextPositionAsync() };
        Apply(project, request);
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return ToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(int id, SaveProjectRequest request)
    {
        var project = await FindMineAsync(id);
        Apply(project, request);
        project.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        return ToDto(project);
    }

    public async Task DeleteAsync(int id)
    {
        db.Projects.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderRequest request)
    {
        await db.Projects.OwnedBy(currentUser.RequireUserId()).ApplyOrderAsync(request);
        await db.SaveChangesAsync();
    }

    private async Task<Project> FindMineAsync(int id) =>
        await db.Projects.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NotFoundException("找不到這個專案");

    private static void Apply(Project p, SaveProjectRequest r)
    {
        p.Name = r.Name.Trim();
        p.Description = r.Description ?? "";
        p.Color = r.Color ?? "";
    }

    private static ProjectDto ToDto(Project p) => new(p.Id, p.Name, p.Description, p.Color, p.SortOrder);
}
