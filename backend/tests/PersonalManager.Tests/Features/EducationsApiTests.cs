using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class EducationsApiTests(ApiFactory factory) : OwnedCollectionContract(factory)
{
    protected override string Resource => "educations";

    protected override object NewItem(string label, bool isPublic = true) =>
        new { school = label, degree = "學士", fieldOfStudy = "視覺傳達設計", startYear = 2014, endYear = 2018, isPublic };

    protected override string LabelOf(JsonElement item) => item.GetProperty("school").GetString()!;

    [Fact]
    public async Task Create_WithoutSchool_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl, new { school = "", degree = "學士" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_EndYearBeforeStartYear_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl, new { school = "某大學", startYear = 2020, endYear = 2016 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InProgressEducation_HasNoEndYear()
    {
        var me = await Factory.CreateUserAsync();

        var education = await CreateAsync(me, new { school = "在學中", startYear = 2024 });

        Assert.Equal(JsonValueKind.Null, education.GetProperty("endYear").ValueKind);
    }
}
