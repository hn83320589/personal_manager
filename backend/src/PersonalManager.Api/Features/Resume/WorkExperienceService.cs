using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Resume;

public sealed class WorkExperienceService(ApplicationDbContext db, ICurrentUser currentUser)
{
    public async Task<List<PublicWorkExperienceDto>> GetPublicAsync(string username)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        return await db.WorkExperiences.AsNoTracking().OwnedBy(userId)
            .Where(w => w.IsPublic)
            .OrderBy(w => w.SortOrder).ThenBy(w => w.Id)
            .Select(w => new PublicWorkExperienceDto(w.Id, w.Company, w.Position, w.StartDate, w.EndDate, w.IsCurrent, w.Description))
            .ToListAsync();
    }

    public Task<List<WorkExperienceDto>> GetMineAsync() =>
        db.WorkExperiences.AsNoTracking().OwnedBy(currentUser.RequireUserId())
            .OrderBy(w => w.SortOrder).ThenBy(w => w.Id)
            .Select(w => ToDto(w))
            .ToListAsync();

    public async Task<WorkExperienceDto> CreateAsync(SaveWorkExperienceRequest request)
    {
        var userId = currentUser.RequireUserId();
        var experience = new WorkExperience
        {
            UserId = userId, SortOrder = await db.WorkExperiences.OwnedBy(userId).NextPositionAsync()
        };
        Apply(experience, request);
        db.WorkExperiences.Add(experience);
        await db.SaveChangesAsync();
        return ToDto(experience);
    }

    public async Task<WorkExperienceDto> UpdateAsync(int id, SaveWorkExperienceRequest request)
    {
        var experience = await FindMineAsync(id);
        Apply(experience, request);
        experience.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToDto(experience);
    }

    public async Task DeleteAsync(int id)
    {
        db.WorkExperiences.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderRequest request)
    {
        var items = await db.WorkExperiences.OwnedBy(currentUser.RequireUserId()).ToListAsync();
        items.ApplyOrder(request);
        await db.SaveChangesAsync();
    }

    private async Task<WorkExperience> FindMineAsync(int id) =>
        await db.WorkExperiences.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(w => w.Id == id)
        ?? throw new NotFoundException("找不到這筆經歷");

    private static void Apply(WorkExperience w, SaveWorkExperienceRequest r)
    {
        w.Company = r.Company.Trim();
        w.Position = r.Position.Trim();
        w.StartDate = r.StartDate;
        w.IsCurrent = r.IsCurrent;
        w.EndDate = r.IsCurrent ? null : r.EndDate;
        w.Description = r.Description ?? "";
        w.IsPublic = r.IsPublic;
    }

    private static WorkExperienceDto ToDto(WorkExperience w) =>
        new(w.Id, w.Company, w.Position, w.StartDate, w.EndDate, w.IsCurrent, w.Description, w.IsPublic, w.SortOrder);
}
