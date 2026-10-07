using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Resume;

public sealed class EducationService(ApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<List<PublicEducationDto>> GetPublicAsync(string username)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        return await db.Educations.AsNoTracking().OwnedBy(userId)
            .Where(e => e.IsPublic)
            .OrderBy(e => e.SortOrder).ThenBy(e => e.Id)
            .Select(e => new PublicEducationDto(e.Id, e.School, e.Degree, e.FieldOfStudy, e.StartYear, e.EndYear, e.Description))
            .ToListAsync();
    }

    public Task<List<EducationDto>> GetMineAsync() =>
        db.Educations.AsNoTracking().OwnedBy(currentUser.RequireUserId())
            .OrderBy(e => e.SortOrder).ThenBy(e => e.Id)
            .Select(e => ToDto(e))
            .ToListAsync();

    public async Task<EducationDto> CreateAsync(SaveEducationRequest request)
    {
        var userId = currentUser.RequireUserId();
        var education = new Education { UserId = userId, SortOrder = await db.Educations.OwnedBy(userId).NextPositionAsync() };
        Apply(education, request);
        db.Educations.Add(education);
        await db.SaveChangesAsync();
        return ToDto(education);
    }

    public async Task<EducationDto> UpdateAsync(int id, SaveEducationRequest request)
    {
        var education = await FindMineAsync(id);
        Apply(education, request);
        education.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        return ToDto(education);
    }

    public async Task DeleteAsync(int id)
    {
        db.Educations.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderRequest request)
    {
        await db.Educations.OwnedBy(currentUser.RequireUserId()).ApplyOrderAsync(request);
        await db.SaveChangesAsync();
    }

    private async Task<Education> FindMineAsync(int id) =>
        await db.Educations.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(e => e.Id == id)
        ?? throw new NotFoundException("找不到這筆學歷");

    private static void Apply(Education e, SaveEducationRequest r)
    {
        e.School = r.School.Trim();
        e.Degree = r.Degree?.Trim() ?? "";
        e.FieldOfStudy = r.FieldOfStudy?.Trim() ?? "";
        e.StartYear = r.StartYear;
        e.EndYear = r.EndYear;
        e.Description = r.Description ?? "";
        e.IsPublic = r.IsPublic;
    }

    private static EducationDto ToDto(Education e) =>
        new(e.Id, e.School, e.Degree, e.FieldOfStudy, e.StartYear, e.EndYear, e.Description, e.IsPublic, e.SortOrder);
}
