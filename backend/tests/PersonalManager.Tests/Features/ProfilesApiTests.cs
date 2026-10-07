using System.Net;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class ProfilesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private sealed record ProfileBody(
        string FullName, string? Title = null, string? Summary = null, string? Description = null,
        string? ProfileImageUrl = null, string? Website = null, string? Location = null,
        string ThemeColor = "blue", string? AvailabilityStatus = null,
        string PortfolioMode = "Designer", string CardStyle = "Visual", string CardRatio = "Portrait",
        string SkillDisplay = "NameOnly");

    private sealed record Profile(
        string Username, string FullName, string Title, string Summary, string Description, string ProfileImageUrl,
        string Website, string Location, string ThemeColor, string AvailabilityStatus,
        string PortfolioMode, string CardStyle, string CardRatio, string SkillDisplay);

    private sealed record DirectoryCard(string Username, string FullName, string Title, string Summary,
        string ProfileImageUrl, string Location, string ThemeColor);

    // ---------- public profile ----------

    [Fact]
    public async Task PublicProfile_UserWithoutSavedProfile_ReturnsDefaults()
    {
        var user = await factory.CreateUserAsync();

        var response = await factory.CreateClient().GetAsync($"/api/public/users/{user.Username}");
        var profile = await response.ReadDataAsync<Profile>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((user.Username, "blue", "Designer", "Visual"),
            (profile.Username, profile.ThemeColor, profile.PortfolioMode, profile.CardStyle));
    }

    [Fact]
    public async Task PublicProfile_NeverContainsEmail()
    {
        var user = await factory.CreateUserAsync();

        var json = await factory.CreateClient().GetStringAsync($"/api/public/users/{user.Username}");

        Assert.DoesNotContain("email", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("@test.local", json);
    }

    [Fact]
    public async Task PublicProfile_InactiveUser_IsNotFound()
    {
        var inactive = await factory.CreateUserAsync(isActive: false);

        var response = await factory.CreateClient().GetAsync($"/api/public/users/{inactive.Username}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- directory ----------

    [Fact]
    public async Task Directory_ListsActiveUsersOnlyWithoutEmail()
    {
        var active = await factory.CreateUserAsync();
        var inactive = await factory.CreateUserAsync(isActive: false);

        var response = await factory.CreateClient().GetAsync("/api/public/users?pageSize=100");
        var json = await response.Content.ReadAsStringAsync();
        var page = await response.ReadDataAsync<Paged<DirectoryCard>>();

        Assert.Contains(page.Items, c => c.Username == active.Username);
        Assert.DoesNotContain(page.Items, c => c.Username == inactive.Username);
        Assert.DoesNotContain("@test.local", json);
    }

    [Fact]
    public async Task Directory_SearchMatchesNameAndTitle()
    {
        var designer = await factory.CreateUserAsync();
        await designer.Client().PutJsonAsync("/api/me/profile",
            new ProfileBody("陳映青", Title: "獨立平面設計師"));
        await factory.CreateUserAsync();

        var byTitle = await (await factory.CreateClient().GetAsync("/api/public/users?q=平面設計"))
            .ReadDataAsync<Paged<DirectoryCard>>();
        var byName = await (await factory.CreateClient().GetAsync("/api/public/users?q=映青"))
            .ReadDataAsync<Paged<DirectoryCard>>();

        Assert.Equal(designer.Username, Assert.Single(byTitle.Items).Username);
        Assert.Equal(designer.Username, Assert.Single(byName.Items).Username);
    }

    [Fact]
    public async Task Directory_IsPaged()
    {
        for (var i = 0; i < 3; i++)
            await factory.CreateUserAsync();

        var page = await (await factory.CreateClient().GetAsync("/api/public/users?page=1&pageSize=2"))
            .ReadDataAsync<Paged<DirectoryCard>>();

        Assert.Equal(2, page.Items.Count);
        Assert.True(page.TotalCount >= 3);
    }

    // ---------- me ----------

    [Fact]
    public async Task Me_Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_Save_IsVisibleOnPublicProfileAndUpdatesFullName()
    {
        var me = await factory.CreateUserAsync();

        var save = await me.Client().PutJsonAsync("/api/me/profile", new ProfileBody(
            "張凱文", Title: "後端工程師", Location: "新竹", ThemeColor: "green",
            AvailabilityStatus: "開放後端職缺", PortfolioMode: "Backend", CardStyle: "Tech", SkillDisplay: "Years"));
        var profile = await (await factory.CreateClient().GetAsync($"/api/public/users/{me.Username}"))
            .ReadDataAsync<Profile>();

        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        Assert.Equal(("張凱文", "後端工程師", "green", "Backend", "Tech", "Years", "開放後端職缺"),
            (profile.FullName, profile.Title, profile.ThemeColor, profile.PortfolioMode, profile.CardStyle,
             profile.SkillDisplay, profile.AvailabilityStatus));
    }

    [Fact]
    public async Task Me_SaveTwice_KeepsSingleProfile()
    {
        var me = await factory.CreateUserAsync();

        await me.Client().PutJsonAsync("/api/me/profile", new ProfileBody("第一次", Title: "A"));
        await me.Client().PutJsonAsync("/api/me/profile", new ProfileBody("第二次", Title: "B"));
        var profile = await (await me.Client().GetAsync("/api/me/profile")).ReadDataAsync<Profile>();

        Assert.Equal(("第二次", "B"), (profile.FullName, profile.Title));
    }

    [Theory]
    [InlineData("orange", "Designer")]
    [InlineData("blue", "Astronaut")]
    public async Task Me_Save_WithUnknownThemeOrMode_IsBadRequest(string theme, string mode)
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PutJsonAsync("/api/me/profile",
            new ProfileBody("名字", ThemeColor: theme, PortfolioMode: mode));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_Save_WithoutFullName_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PutJsonAsync("/api/me/profile", new ProfileBody(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
