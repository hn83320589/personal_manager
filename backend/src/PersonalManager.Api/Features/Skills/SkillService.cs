using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Skills;

public sealed class SkillService(ApplicationDbContext db, ICurrentUser currentUser)
{
    public async Task<List<PublicSkillDto>> GetPublicAsync(string username)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        return await db.Skills.AsNoTracking()
            .OwnedBy(userId)
            .Where(s => s.IsPublic)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Id)
            .Select(s => new PublicSkillDto(s.Id, s.Name, s.Category, s.Level, s.YearsOfExperience))
            .ToListAsync();
    }

    public Task<List<SkillDto>> GetMineAsync() =>
        db.Skills.AsNoTracking()
            .OwnedBy(currentUser.RequireUserId())
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Id)
            .Select(s => ToDto(s))
            .ToListAsync();

    public async Task<SkillDto> CreateAsync(SaveSkillRequest request)
    {
        var userId = currentUser.RequireUserId();
        var skill = new Skill { UserId = userId, SortOrder = await db.Skills.OwnedBy(userId).NextPositionAsync() };
        Apply(skill, request);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        return ToDto(skill);
    }

    public async Task<SkillDto> UpdateAsync(int id, SaveSkillRequest request)
    {
        var skill = await FindMineAsync(id);
        Apply(skill, request);
        skill.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToDto(skill);
    }

    public async Task DeleteAsync(int id)
    {
        db.Skills.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderRequest request)
    {
        await db.Skills.OwnedBy(currentUser.RequireUserId()).ApplyOrderAsync(request);
        await db.SaveChangesAsync();
    }

    private async Task<Skill> FindMineAsync(int id) =>
        await db.Skills.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(s => s.Id == id)
        ?? throw new NotFoundException("找不到這項技能");

    private static void Apply(Skill skill, SaveSkillRequest request)
    {
        skill.Name = request.Name.Trim();
        skill.Category = request.Category?.Trim() ?? string.Empty;
        skill.Level = request.Level;
        skill.YearsOfExperience = request.YearsOfExperience;
        skill.IsPublic = request.IsPublic;
    }

    private static SkillDto ToDto(Skill s) =>
        new(s.Id, s.Name, s.Category, s.Level, s.YearsOfExperience, s.IsPublic, s.SortOrder);
}
