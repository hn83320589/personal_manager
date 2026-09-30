using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Profiles;

public sealed class ProfileService(ApplicationDbContext db, ICurrentUser currentUser)
{
    public const int MaxPageSize = 100;

    public Task<PagedResult<DirectoryCardDto>> GetDirectoryAsync(string? keyword, int page, int pageSize)
    {
        var query =
            from user in db.Users.AsNoTracking()
            where user.IsActive
            join p in db.PersonalProfiles on user.Id equals p.UserId into profiles
            from profile in profiles.DefaultIfEmpty()
            select new { user, profile };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(x => x.user.Username.Contains(k) || x.user.FullName.Contains(k)
                                     || (x.profile != null && x.profile.Title.Contains(k)));
        }

        return query
            .OrderBy(x => x.user.Id)
            .Select(x => new DirectoryCardDto(
                x.user.Username, x.user.FullName,
                x.profile != null ? x.profile.Title : "",
                x.profile != null ? x.profile.Summary : "",
                x.profile != null ? x.profile.ProfileImageUrl : "",
                x.profile != null ? x.profile.Location : "",
                x.profile != null ? x.profile.ThemeColor : "blue"))
            .ToPagedResultAsync(page, pageSize, MaxPageSize);
    }

    public async Task<ProfileDto> GetPublicAsync(string username)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        return await LoadAsync(userId);
    }

    public Task<ProfileDto> GetMineAsync() => LoadAsync(currentUser.RequireUserId());

    /// <summary>個人資料只有一份：沒有就建立，有就覆寫。</summary>
    public async Task<ProfileDto> SaveAsync(SaveProfileRequest request)
    {
        var userId = currentUser.RequireUserId();
        var user = await db.Users.FirstAsync(u => u.Id == userId);
        var profile = await db.PersonalProfiles.OwnedBy(userId).FirstOrDefaultAsync();
        if (profile is null)
        {
            profile = new PersonalProfile { UserId = userId };
            db.PersonalProfiles.Add(profile);
        }

        user.FullName = request.FullName.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        profile.Title = request.Title?.Trim() ?? "";
        profile.Summary = request.Summary?.Trim() ?? "";
        profile.Description = request.Description ?? "";
        profile.ProfileImageUrl = request.ProfileImageUrl?.Trim() ?? "";
        profile.Website = request.Website?.Trim() ?? "";
        profile.Location = request.Location?.Trim() ?? "";
        profile.ThemeColor = request.ThemeColor;
        profile.AvailabilityStatus = request.AvailabilityStatus?.Trim() ?? "";
        profile.PortfolioMode = request.PortfolioMode;
        profile.CardStyle = request.CardStyle;
        profile.CardRatio = request.CardRatio;
        profile.SkillDisplay = request.SkillDisplay;
        profile.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return ToDto(user, profile);
    }

    /// <summary>尚未儲存過個人資料時，以預設設定回傳，前台不需要處理「沒有個人資料」的情況。</summary>
    private async Task<ProfileDto> LoadAsync(int userId)
    {
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        var profile = await db.PersonalProfiles.AsNoTracking().OwnedBy(userId).FirstOrDefaultAsync()
                      ?? new PersonalProfile { UserId = userId };
        return ToDto(user, profile);
    }

    private static ProfileDto ToDto(User user, PersonalProfile p) => new(
        user.Username, user.FullName, p.Title, p.Summary, p.Description, p.ProfileImageUrl, p.Website, p.Location,
        p.ThemeColor, p.AvailabilityStatus, p.PortfolioMode, p.CardStyle, p.CardRatio, p.SkillDisplay);
}
