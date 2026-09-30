using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class WorkExperiencesApiTests(ApiFactory factory) : OwnedCollectionContract(factory)
{
    protected override string Resource => "work-experiences";

    protected override object NewItem(string label, bool isPublic = true) =>
        new { company = label, position = "平面設計師", startDate = "2018-07-01", endDate = "2020-12-31", isPublic };

    protected override string LabelOf(JsonElement item) => item.GetProperty("company").GetString()!;

    [Fact]
    public async Task Create_WithoutCompanyOrPosition_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var noCompany = await me.Client().PostJsonAsync(MeUrl, new { company = "", position = "設計師" });
        var noPosition = await me.Client().PostJsonAsync(MeUrl, new { company = "白日設計", position = "" });

        Assert.Equal(HttpStatusCode.BadRequest, noCompany.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noPosition.StatusCode);
    }

    [Fact]
    public async Task Create_EndDateBeforeStartDate_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl,
            new { company = "白日設計", position = "設計師", startDate = "2020-01-01", endDate = "2019-01-01" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_CurrentJob_IgnoresEndDate()
    {
        var me = await Factory.CreateUserAsync();

        var job = await CreateAsync(me, new
        {
            company = "映青設計工作室", position = "獨立設計師", startDate = "2021-01-01", endDate = "2023-01-01", isCurrent = true
        });

        Assert.True(job.GetProperty("isCurrent").GetBoolean());
        Assert.Equal(JsonValueKind.Null, job.GetProperty("endDate").ValueKind);
    }
}
