using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PersonalManager.Api.Data;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public partial class AuthApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "correct horse battery";
    private const string CookieName = "pm_refresh";

    /// <summary>refresh cookie 設有 Secure，用 https 的 client 才會被保存與送出。</summary>
    private HttpClient Browser() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = true
    });

    private static string NewUsername() => $"u{Guid.NewGuid():N}"[..16];

    private static async Task<HttpResponseMessage> Register(HttpClient client, string username, string? email = null,
        string password = Password) =>
        await client.PostJsonAsync("/api/auth/register",
            new { username, email = email ?? $"{username}@test.local", password, fullName = "測試使用者" });

    private static Task<HttpResponseMessage> Login(HttpClient client, string username, string password = Password) =>
        client.PostJsonAsync("/api/auth/login", new { username, password });

    private static Task<HttpResponseMessage> Refresh(HttpClient client) => client.PostAsync("/api/auth/refresh", null);

    private static async Task<string> AccessToken(HttpResponseMessage response) =>
        (await response.ReadDataAsync<JsonElement>()).GetProperty("accessToken").GetString()!;

    private static string RefreshCookieHeader(HttpResponseMessage response) =>
        response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith(CookieName + "="));

    private static string RefreshTokenValue(HttpResponseMessage response) =>
        RefreshCookieHeader(response).Split(';')[0][(CookieName.Length + 1)..];

    private async Task<(HttpClient Browser, string Username)> RegisteredBrowser()
    {
        var browser = Browser();
        var username = NewUsername();
        Assert.Equal(HttpStatusCode.Created, (await Register(browser, username)).StatusCode);
        return (browser, username);
    }

    private HttpClient WithBearer(string accessToken)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    // ---------- login ----------

    [Fact]
    public async Task Login_ReturnsAccessTokenAndSetsHardenedRefreshCookie()
    {
        var (_, username) = await RegisteredBrowser();

        var response = await Login(Browser(), username);
        var body = await response.Content.ReadAsStringAsync();
        var cookie = RefreshCookieHeader(response).ToLowerInvariant();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrEmpty(await AccessToken(response)));
        Assert.Contains("httponly", cookie);
        Assert.Contains("secure", cookie);
        Assert.Contains("samesite=strict", cookie);
        Assert.Contains("path=/api/auth", cookie);
        Assert.DoesNotContain(RefreshTokenValue(response), body);
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownUser_GiveTheSameAnswer()
    {
        var (_, username) = await RegisteredBrowser();

        var wrongPassword = await Login(Browser(), username, "not the password");
        var unknownUser = await Login(Browser(), "nobody-" + username);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
        Assert.Equal(await wrongPassword.Content.ReadAsStringAsync(), await unknownUser.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_DeactivatedUser_IsRejected()
    {
        var inactive = await factory.CreateUserAsync(isActive: false);

        var response = await Login(Browser(), inactive.Username, "integration-test-password");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessToken_GivesAccessToCurrentUser()
    {
        var (_, username) = await RegisteredBrowser();
        var token = await AccessToken(await Login(Browser(), username));

        var me = await (await WithBearer(token).GetAsync("/api/auth/me")).ReadDataAsync<JsonElement>();

        Assert.Equal(username, me.GetProperty("username").GetString());
    }

    // ---------- refresh ----------

    [Fact]
    public async Task Refresh_WithCookie_IssuesNewAccessTokenAndRotatesCookie()
    {
        var (browser, _) = await RegisteredBrowser();

        var first = await Refresh(browser);
        var second = await Refresh(browser);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.NotEqual(RefreshTokenValue(first), RefreshTokenValue(second));
    }

    [Fact]
    public async Task Refresh_WithoutCookie_ReturnsNoContentBecauseNobodyIsSignedIn()
    {
        var response = await Refresh(Browser());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithUnknownCookie_IsUnauthorized()
    {
        var browser = Browser();
        browser.DefaultRequestHeaders.Add("Cookie", $"{CookieName}=not-a-real-token");

        var response = await Refresh(browser);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReusingARotatedToken_RevokesTheWholeSession()
    {
        var (browser, username) = await RegisteredBrowser();
        var stolen = RefreshTokenValue(await Login(browser, username));
        await Refresh(browser);   // 合法使用者先輪換，stolen 已失效

        var attacker = Browser();
        attacker.DefaultRequestHeaders.Add("Cookie", $"{CookieName}={stolen}");
        var reuse = await Refresh(attacker);
        var legitimateAfterReuse = await Refresh(browser);

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, legitimateAfterReuse.StatusCode);
    }

    [Fact]
    public async Task Refresh_AfterUserIsDeactivated_IsRejected()
    {
        var (browser, username) = await RegisteredBrowser();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Users.Single(u => u.Username == username).IsActive = false;
            await db.SaveChangesAsync();
        }

        var response = await Refresh(browser);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshTokens_AreStoredOnlyAsHashes()
    {
        var browser = Browser();
        var username = NewUsername();
        var raw = RefreshTokenValue(await Register(browser, username));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = db.RefreshTokens.Select(t => t.TokenHash).ToList();

        Assert.DoesNotContain(raw, stored);
        Assert.All(stored, hash => Assert.Matches("^[0-9a-f]{64}$", hash));
    }

    // ---------- logout ----------

    [Fact]
    public async Task Logout_RevokesRefreshTokenAndClearsCookie()
    {
        var browser = Browser();
        var token = RefreshTokenValue(await Register(browser, NewUsername()));

        var logout = await browser.PostAsync("/api/auth/logout", null);
        var refreshAfterLogout = await Refresh(browser);
        var replay = Browser();
        replay.DefaultRequestHeaders.Add("Cookie", $"{CookieName}={token}");
        var replayedToken = await Refresh(replay);

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Contains("expires=thu, 01 jan 1970", RefreshCookieHeader(logout).ToLowerInvariant());
        Assert.Equal(HttpStatusCode.NoContent, refreshAfterLogout.StatusCode);   // cookie 已刪除
        Assert.Equal(HttpStatusCode.Unauthorized, replayedToken.StatusCode);     // token 本身也已撤銷
    }

    // ---------- register ----------

    [Theory]
    [InlineData("ab")]
    [InlineData("has space")]
    [InlineData("中文帳號")]
    [InlineData("slash/name")]
    public async Task Register_UsernameUnsuitableForProfileUrl_IsBadRequest(string username)
    {
        var response = await Register(Browser(), username);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_IsBadRequest()
    {
        var response = await Register(Browser(), NewUsername(), password: "short");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateUsernameOrEmail_IsConflict()
    {
        var (_, username) = await RegisteredBrowser();

        var sameUsername = await Register(Browser(), username.ToUpperInvariant(), email: "other@test.local");
        var sameEmail = await Register(Browser(), NewUsername(), email: $"{username}@test.local");

        Assert.Equal(HttpStatusCode.Conflict, sameUsername.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, sameEmail.StatusCode);
    }

    // ---------- password reset ----------

    private async Task<string> RequestResetToken(string email)
    {
        await Browser().PostJsonAsync("/api/auth/forgot-password", new { email });
        var mail = factory.Outbox.SentTo(email).Last();
        return ResetToken().Match(mail.HtmlBody).Groups[1].Value;
    }

    [GeneratedRegex(@"token=([A-Za-z0-9_\-]+)")]
    private static partial Regex ResetToken();

    [Fact]
    public async Task ForgotPassword_UnknownEmail_LooksTheSameAndSendsNothing()
    {
        var email = $"{NewUsername()}@nowhere.local";

        var response = await Browser().PostJsonAsync("/api/auth/forgot-password", new { email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(factory.Outbox.SentTo(email));
    }

    [Fact]
    public async Task ResetPassword_ChangesPasswordAndEndsExistingSessions()
    {
        var (browser, username) = await RegisteredBrowser();
        var token = await RequestResetToken($"{username}@test.local");

        var reset = await Browser().PostJsonAsync("/api/auth/reset-password", new { token, newPassword = "a brand new password" });
        var oldPassword = await Login(Browser(), username);
        var newPassword = await Login(Browser(), username, "a brand new password");
        var oldSession = await Refresh(browser);

        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, oldPassword.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, oldSession.StatusCode);
    }

    [Fact]
    public async Task ResetToken_CannotBeUsedTwice()
    {
        var (_, username) = await RegisteredBrowser();
        var token = await RequestResetToken($"{username}@test.local");

        await Browser().PostJsonAsync("/api/auth/reset-password", new { token, newPassword = "first new password" });
        var second = await Browser().PostJsonAsync("/api/auth/reset-password", new { token, newPassword = "second new password" });

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task ResetToken_ExpiresAfterOneHour()
    {
        using var isolated = ApiFactory.For("Testing");
        var client = isolated.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var username = NewUsername();
        await Register(client, username);
        await client.PostJsonAsync("/api/auth/forgot-password", new { email = $"{username}@test.local" });
        var token = ResetToken().Match(isolated.Outbox.SentTo($"{username}@test.local").Single().HtmlBody).Groups[1].Value;

        isolated.Clock.Advance(TimeSpan.FromMinutes(61));
        var response = await client.PostJsonAsync("/api/auth/reset-password", new { token, newPassword = "too late password" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        SqliteConnection.ClearAllPools();
    }

    // ---------- change password ----------

    [Fact]
    public async Task ChangePassword_RequiresCurrentPassword()
    {
        var (browser, username) = await RegisteredBrowser();
        var token = await AccessToken(await Login(browser, username));

        var response = await WithBearer(token).PutJsonAsync("/api/me/password",
            new { currentPassword = "wrong current", newPassword = "another new password" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_EndsOtherSessions()
    {
        var (otherDevice, username) = await RegisteredBrowser();
        var token = await AccessToken(await Login(Browser(), username));

        var change = await WithBearer(token).PutJsonAsync("/api/me/password",
            new { currentPassword = Password, newPassword = "another new password" });
        var otherDeviceRefresh = await Refresh(otherDevice);

        Assert.Equal(HttpStatusCode.OK, change.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, otherDeviceRefresh.StatusCode);
    }
}

/// <summary>
/// 登入有嚴格的次數限制（防暴力破解），但前端每次開啟頁面都會呼叫 refresh 還原登入；
/// 兩者若共用同一個額度，多開幾個分頁就會被登出。
/// </summary>
public class AuthRateLimitTests
{
    private static ApiFactory LowLoginLimit() => ApiFactory.For("Testing", settings: new Dictionary<string, string?>
    {
        ["RateLimiting:AuthPermitsPerMinute"] = "2",
        ["RateLimiting:SessionPermitsPerMinute"] = "20",
    });

    private static HttpClient Browser(ApiFactory factory) => factory.CreateClient(
        new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

    [Fact]
    public async Task TooManyLoginAttempts_AreRejectedWith429()
    {
        using var factory = LowLoginLimit();
        var client = factory.CreateClient();
        var body = new { username = "nobody", password = "wrong-password" };

        await client.PostJsonAsync("/api/auth/login", body);
        await client.PostJsonAsync("/api/auth/login", body);
        var third = await client.PostJsonAsync("/api/auth/login", body);

        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }

    [Fact]
    public async Task RestoringTheSessionRepeatedly_DoesNotUseUpTheLoginLimit()
    {
        using var factory = LowLoginLimit();
        var browser = Browser(factory);
        await browser.PostJsonAsync("/api/auth/register",
            new { username = "tabs", email = "tabs@test.local", password = "correct horse battery", fullName = "多分頁" });

        var refreshes = new List<HttpStatusCode>();
        for (var i = 0; i < 5; i++)
            refreshes.Add((await browser.PostAsync("/api/auth/refresh", null)).StatusCode);

        Assert.All(refreshes, status => Assert.Equal(HttpStatusCode.OK, status));
    }
}
