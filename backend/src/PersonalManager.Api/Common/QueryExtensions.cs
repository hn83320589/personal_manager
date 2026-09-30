using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Common;

public static class QueryExtensions
{
    /// <summary>
    /// 只取目前使用者自己的資料。存取別人的資料時會查不到而回 404，
    /// 而不是 403，避免透露「這個 ID 存在，只是不屬於你」。
    /// </summary>
    public static IQueryable<T> OwnedBy<T>(this IQueryable<T> query, int userId) where T : IOwnedByUser =>
        query.Where(e => e.UserId == userId);

    /// <summary>公開頁面的使用者：必須存在且為啟用狀態，否則視為找不到。</summary>
    public static async Task<int> RequirePublicUserIdAsync(this ApplicationDbContext db, string username)
    {
        var userId = await db.Users
            .Where(u => u.Username == username && u.IsActive)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();
        return userId ?? throw new NotFoundException("找不到這位使用者");
    }
}
