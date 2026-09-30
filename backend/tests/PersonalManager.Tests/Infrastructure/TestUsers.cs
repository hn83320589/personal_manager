using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using PersonalManager.Api.Data;
using PersonalManager.Api.Features.Auth;
using PersonalManager.Api.Models;

namespace PersonalManager.Tests.Infrastructure;

/// <summary>測試用的已登入使用者，<see cref="Client"/> 會自動帶上 Bearer token。</summary>
public sealed record TestUser(int Id, string Username, string Token, ApiFactory Factory)
{
    public HttpClient Client()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        return client;
    }
}

public static class TestUsers
{
    private const string Password = "integration-test-password";

    /// <summary>
    /// 直接寫入資料庫建立使用者，再以 <see cref="AuthService"/> 取得 token。
    /// 不經過註冊／登入 API，避免測試數量多時觸發流量限制。
    /// </summary>
    public static async Task<TestUser> CreateUserAsync(this ApiFactory factory, string? username = null,
        bool isActive = true, string role = "User")
    {
        username ??= $"user_{Guid.NewGuid():N}"[..20];
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = new User
        {
            Username = username,
            Email = $"{username}@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password, workFactor: 4),
            FullName = username,
            Role = role,
            IsActive = isActive
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // 停用的帳號無法登入；回傳沒有 token 的使用者，用於測試公開頁面與登入不接受停用帳號
        if (!isActive)
            return new TestUser(user.Id, username, "", factory);

        var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
        var session = await auth.LoginAsync(new LoginRequest(username, Password));
        return new TestUser(user.Id, username, session.Access.AccessToken, factory);
    }
}
