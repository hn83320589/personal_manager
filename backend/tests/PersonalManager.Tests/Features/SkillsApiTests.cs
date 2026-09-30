using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class SkillsApiTests(ApiFactory factory) : PublicCollectionContract(factory)
{
    protected override string Resource => "skills";

    protected override object NewItem(string label, bool isPublic = true) =>
        new { name = label, category = "設計", isPublic };

    protected override string LabelOf(JsonElement item) => item.GetProperty("name").GetString()!;

    [Fact]
    public async Task Create_LevelAndYearsAreOptional()
    {
        var me = await Factory.CreateUserAsync();

        var skill = await CreateAsync(me, new { name = "品牌識別" });

        Assert.Equal(JsonValueKind.Null, skill.GetProperty("level").ValueKind);
        Assert.Equal(JsonValueKind.Null, skill.GetProperty("yearsOfExperience").ValueKind);
    }

    [Fact]
    public async Task Create_AppendsToEndOfDisplayOrder()
    {
        var me = await Factory.CreateUserAsync();

        var first = await CreateAsync(me, NewItem("A"));
        var second = await CreateAsync(me, NewItem("B"));

        Assert.True(second.GetProperty("sortOrder").GetInt32() > first.GetProperty("sortOrder").GetInt32());
    }

    [Fact]
    public async Task Create_WithoutName_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl, new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_StoresLevelAndYears()
    {
        var me = await Factory.CreateUserAsync();
        var skill = await CreateAsync(me, NewItem("Vue"));

        var updated = await (await me.Client().PutJsonAsync($"{MeUrl}/{skill.GetProperty("id").GetInt32()}",
            new { name = "Vue 3", level = "Expert", yearsOfExperience = 5 })).ReadDataAsync<JsonElement>();

        Assert.Equal(("Expert", 5), (updated.GetProperty("level").GetString(), updated.GetProperty("yearsOfExperience").GetInt32()));
    }
}
