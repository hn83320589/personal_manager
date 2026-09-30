using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Admin;

public sealed record AdminUserDto(
    int Id, string Username, string Email, string FullName, string Role, bool IsActive, DateTime CreatedAt);

public sealed record SetUserStatusRequest(bool IsActive);

public sealed record SetUserRoleRequest([Required] string Role);

/// <summary>
/// 管理員管理使用者。不提供刪除：使用者的資料分散在多張資料表，停用即可讓帳號無法登入且公開頁面消失。
/// 管理員不能停用或降級自己，避免網站失去最後一位管理員。
/// </summary>
public sealed class AdminUserService(ApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public const int MaxPageSize = 100;

    public Task<PagedResult<AdminUserDto>> GetUsersAsync(string? keyword, int page, int pageSize)
    {
        var users = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            users = users.Where(u => u.Username.Contains(k) || u.Email.Contains(k) || u.FullName.Contains(k));
        }
        return users
            .OrderBy(u => u.Id)
            .Select(u => new AdminUserDto(u.Id, u.Username, u.Email, u.FullName, u.Role, u.IsActive, u.CreatedAt))
            .ToPagedResultAsync(page, pageSize, MaxPageSize);
    }

    public async Task SetStatusAsync(int userId, bool isActive)
    {
        if (!isActive && userId == currentUser.RequireUserId())
            throw new DomainValidationException("不能停用自己的帳號");

        var user = await FindAsync(userId);
        user.IsActive = isActive;
        user.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();

        // 停用時立即結束對方所有登入中的工作階段
        if (!isActive)
            await db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true));
    }

    public async Task SetRoleAsync(int userId, string role)
    {
        if (!Roles.All.Contains(role))
            throw new DomainValidationException($"角色只能是 {string.Join("、", Roles.All)}");
        if (userId == currentUser.RequireUserId() && role != Roles.Admin)
            throw new DomainValidationException("不能移除自己的管理員權限");

        var user = await FindAsync(userId);
        user.Role = role;
        user.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
    }

    private async Task<User> FindAsync(int userId) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new NotFoundException("找不到這位使用者");
}

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/users")]
public sealed class AdminUsersController(AdminUserService users) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<PagedResult<AdminUserDto>>> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        ApiResponse<PagedResult<AdminUserDto>>.Ok(await users.GetUsersAsync(q, page, pageSize));

    [HttpPut("{id:int}/status")]
    public async Task<ApiResponse> SetStatus(int id, SetUserStatusRequest request)
    {
        await users.SetStatusAsync(id, request.IsActive);
        return ApiResponse.Ok(request.IsActive ? "已啟用帳號" : "已停用帳號");
    }

    [HttpPut("{id:int}/role")]
    public async Task<ApiResponse> SetRole(int id, SetUserRoleRequest request)
    {
        await users.SetRoleAsync(id, request.Role);
        return ApiResponse.Ok("已更新角色，對方重新登入後生效");
    }
}
