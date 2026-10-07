using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class AdminUsersApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static string StatusUrl(TestUser user) => $"/api/admin/users/{user.Id}/status";
    private static string RoleUrl(TestUser user) => $"/api/admin/users/{user.Id}/role";

    private async Task<List<JsonElement>> ListAs(TestUser admin, string query) =>
        (await (await admin.Client().GetAsync($"/api/admin/users{query}")).ReadDataAsync<Paged<JsonElement>>()).Items;

    [Fact]
    public async Task Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegularUser_IsForbidden()
    {
        var member = await factory.CreateUserAsync();

        var list = await member.Client().GetAsync("/api/admin/users");
        var deactivate = await member.Client().PutJsonAsync(StatusUrl(member), new { isActive = false });

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deactivate.StatusCode);
    }

    [Fact]
    public async Task Admin_ListsUsersWithoutPasswordHashes()
    {
        var admin = await factory.CreateUserAsync(role: "Admin");
        var member = await factory.CreateUserAsync();

        var response = await admin.Client().GetAsync($"/api/admin/users?q={member.Username}");
        var json = await response.Content.ReadAsStringAsync();
        var users = (await response.ReadDataAsync<Paged<JsonElement>>()).Items;

        Assert.Equal(member.Username, Assert.Single(users).GetProperty("username").GetString());
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Admin_DeactivatingAUser_BlocksLoginAndEndsSessions()
    {
        var admin = await factory.CreateUserAsync(role: "Admin");
        var browser = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var username = $"u{Guid.NewGuid():N}"[..16];
        await browser.PostJsonAsync("/api/auth/register",
            new { username, email = $"{username}@test.local", password = "member password", fullName = "成員" });
        var member = (await ListAs(admin, $"?q={username}")).Single();

        var deactivate = await admin.Client().PutJsonAsync($"/api/admin/users/{member.GetProperty("id").GetInt32()}/status", new { isActive = false });
        var refresh = await browser.PostAsync("/api/auth/refresh", null);
        var login = await browser.PostJsonAsync("/api/auth/login", new { username, password = "member password" });

        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Admin_CanReactivateAUser()
    {
        var admin = await factory.CreateUserAsync(role: "Admin");
        var member = await factory.CreateUserAsync(isActive: false);

        await admin.Client().PutJsonAsync(StatusUrl(member), new { isActive = true });
        var profile = await factory.CreateClient().GetAsync($"/api/public/users/{member.Username}");

        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotDeactivateOrDemoteThemselves()
    {
        var admin = await factory.CreateUserAsync(role: "Admin");

        var deactivate = await admin.Client().PutJsonAsync(StatusUrl(admin), new { isActive = false });
        var demote = await admin.Client().PutJsonAsync(RoleUrl(admin), new { role = "User" });

        Assert.Equal(HttpStatusCode.BadRequest, deactivate.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, demote.StatusCode);
    }

    [Fact]
    public async Task Admin_PromotingAUser_GrantsAdminAccessOnNextLogin()
    {
        var admin = await factory.CreateUserAsync(role: "Admin");
        var member = await factory.CreateUserAsync();

        var promote = await admin.Client().PutJsonAsync(RoleUrl(member), new { role = "Admin" });
        var login = await factory.CreateClient().PostJsonAsync("/api/auth/login",
            new { username = member.Username, password = "integration-test-password" });
        var newToken = (await login.ReadDataAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
        var promoted = member with { Token = newToken };
        var access = await promoted.Client().GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.OK, promote.StatusCode);
        Assert.Equal(HttpStatusCode.OK, access.StatusCode);
    }

    [Fact]
    public async Task Admin_UnknownRole_IsBadRequest()
    {
        var admin = await factory.CreateUserAsync(role: "Admin");
        var member = await factory.CreateUserAsync();

        var response = await admin.Client().PutJsonAsync(RoleUrl(member), new { role = "SuperUser" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_UnknownUser_IsNotFound()
    {
        var admin = await factory.CreateUserAsync(role: "Admin");

        var response = await admin.Client().PutJsonAsync("/api/admin/users/999999/status", new { isActive = false });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class FirstAdminBootstrapTests
{
    [Fact]
    public async Task RegisteringWithAConfiguredEmail_CreatesAnAdmin()
    {
        using var factory = ApiFactory.For("Testing", settings: new Dictionary<string, string?>
        {
            ["Admin:BootstrapEmails:0"] = "owner@example.com"
        });
        var browser = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var owner = await (await browser.PostJsonAsync("/api/auth/register",
            new { username = "owner", email = "Owner@Example.com", password = "owner password", fullName = "站長" })).ReadDataAsync<JsonElement>();
        var other = await (await browser.PostJsonAsync("/api/auth/register",
            new { username = "visitor", email = "visitor@example.com", password = "visitor password", fullName = "訪客" })).ReadDataAsync<JsonElement>();

        Assert.Equal("Admin", owner.GetProperty("user").GetProperty("role").GetString());
        Assert.Equal("User", other.GetProperty("user").GetProperty("role").GetString());
    }
}
