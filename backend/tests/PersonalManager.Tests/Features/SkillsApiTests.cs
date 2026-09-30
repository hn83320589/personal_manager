using System.Net;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class SkillsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private sealed record SkillBody(string Name, string? Category = null, string? Level = null,
        int? YearsOfExperience = null, bool IsPublic = true);

    private sealed record Skill(int Id, string Name, string Category, string? Level, int? YearsOfExperience,
        bool IsPublic, int SortOrder);

    private sealed record PublicSkill(int Id, string Name, string Category, string? Level, int? YearsOfExperience);

    private static async Task<Skill> Create(TestUser user, SkillBody body)
    {
        var response = await user.Client().PostJsonAsync("/api/me/skills", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadDataAsync<Skill>();
    }

    // ---------- public ----------

    [Fact]
    public async Task Public_ReturnsOnlyPublicSkillsInDisplayOrder()
    {
        var owner = await factory.CreateUserAsync();
        await Create(owner, new SkillBody("Figma"));
        await Create(owner, new SkillBody("私人筆記", IsPublic: false));
        await Create(owner, new SkillBody("Illustrator"));

        var response = await factory.CreateClient().GetAsync($"/api/public/users/{owner.Username}/skills");
        var skills = await response.ReadDataAsync<List<PublicSkill>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Figma", "Illustrator"], skills.Select(s => s.Name));
    }

    [Fact]
    public async Task Public_UnknownUser_ReturnsNotFound()
    {
        var response = await factory.CreateClient().GetAsync("/api/public/users/nobody-here/skills");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Public_InactiveUser_ReturnsNotFound()
    {
        var inactive = await factory.CreateUserAsync(isActive: false);

        var response = await factory.CreateClient().GetAsync($"/api/public/users/{inactive.Username}/skills");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- me ----------

    [Fact]
    public async Task Me_Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/skills");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_List_ReturnsOnlyOwnSkillsIncludingPrivate()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        await Create(me, new SkillBody("Go"));
        await Create(me, new SkillBody("內部工具", IsPublic: false));
        await Create(other, new SkillBody("別人的技能"));

        var skills = await (await me.Client().GetAsync("/api/me/skills")).ReadDataAsync<List<Skill>>();

        Assert.Equal(["Go", "內部工具"], skills.Select(s => s.Name));
    }

    [Fact]
    public async Task Me_Create_LevelAndYearsAreOptional()
    {
        var me = await factory.CreateUserAsync();

        var skill = await Create(me, new SkillBody("品牌識別", Category: "設計"));

        Assert.Null(skill.Level);
        Assert.Null(skill.YearsOfExperience);
    }

    [Fact]
    public async Task Me_Create_AppendsToEndOfDisplayOrder()
    {
        var me = await factory.CreateUserAsync();

        var first = await Create(me, new SkillBody("A"));
        var second = await Create(me, new SkillBody("B"));

        Assert.True(second.SortOrder > first.SortOrder);
    }

    [Fact]
    public async Task Me_Create_WithoutName_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync("/api/me/skills", new SkillBody(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_Update_ChangesOwnSkill()
    {
        var me = await factory.CreateUserAsync();
        var skill = await Create(me, new SkillBody("Vue", Level: "Advanced", YearsOfExperience: 3));

        var response = await me.Client().PutJsonAsync($"/api/me/skills/{skill.Id}",
            new SkillBody("Vue 3", Level: "Expert", YearsOfExperience: 5, IsPublic: false));
        var updated = await response.ReadDataAsync<Skill>();

        Assert.Equal(("Vue 3", "Expert", 5, false), (updated.Name, updated.Level, updated.YearsOfExperience, updated.IsPublic));
    }

    [Fact]
    public async Task Me_UpdateOrDeleteSomeoneElsesSkill_IsNotFoundAndLeavesItUnchanged()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        var skill = await Create(owner, new SkillBody("原始名稱"));

        var update = await intruder.Client().PutJsonAsync($"/api/me/skills/{skill.Id}", new SkillBody("被竄改"));
        var delete = await intruder.Client().DeleteAsync($"/api/me/skills/{skill.Id}");
        var ownerView = await (await owner.Client().GetAsync("/api/me/skills")).ReadDataAsync<List<Skill>>();

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal("原始名稱", Assert.Single(ownerView).Name);
    }

    [Fact]
    public async Task Me_Delete_RemovesOwnSkill()
    {
        var me = await factory.CreateUserAsync();
        var skill = await Create(me, new SkillBody("要刪除的"));

        var response = await me.Client().DeleteAsync($"/api/me/skills/{skill.Id}");
        var remaining = await (await me.Client().GetAsync("/api/me/skills")).ReadDataAsync<List<Skill>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task Me_Reorder_AppliesGivenOrder()
    {
        var me = await factory.CreateUserAsync();
        var a = await Create(me, new SkillBody("A"));
        var b = await Create(me, new SkillBody("B"));
        var c = await Create(me, new SkillBody("C"));

        var response = await me.Client().PutJsonAsync("/api/me/skills/order", new { ids = new[] { c.Id, a.Id, b.Id } });
        var skills = await (await me.Client().GetAsync("/api/me/skills")).ReadDataAsync<List<Skill>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["C", "A", "B"], skills.Select(s => s.Name));
    }

    [Fact]
    public async Task Me_Reorder_WithSomeoneElsesSkill_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        var mine = await Create(me, new SkillBody("我的"));
        var theirs = await Create(other, new SkillBody("別人的"));

        var response = await me.Client().PutJsonAsync("/api/me/skills/order", new { ids = new[] { theirs.Id, mine.Id } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
