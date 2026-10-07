using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class CalendarApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private sealed record EventBody(
        string Title, DateTime StartTime, DateTime EndTime, bool IsPublic = true, bool IsAllDay = false,
        string? Color = null, string Recurrence = "None", DateOnly? RecurrenceUntil = null, string? Description = null);

    private static DateTime Utc(int year, int month, int day, int hour = 9) => new(year, month, day, hour, 0, 0, DateTimeKind.Utc);

    private static Task<JsonElement> Create(TestUser user, EventBody body) => user.PostCreatedAsync("/api/me/calendar/events", body);

    private static string Range(DateTime from, DateTime to) => $"?from={from:O}&to={to:O}";

    private async Task<List<JsonElement>> PublicOccurrences(string username, DateTime from, DateTime to) =>
        await (await factory.CreateClient().GetAsync($"/api/public/users/{username}/calendar{Range(from, to)}"))
            .ReadDataAsync<List<JsonElement>>();

    private static async Task<List<JsonElement>> MyOccurrences(TestUser user, DateTime from, DateTime to) =>
        await (await user.Client().GetAsync($"/api/me/calendar{Range(from, to)}")).ReadDataAsync<List<JsonElement>>();

    private static List<DateTime> Starts(IEnumerable<JsonElement> occurrences) =>
        occurrences.Select(o => o.GetProperty("start").GetDateTime().ToUniversalTime()).ToList();

    // ---------- public ----------

    [Fact]
    public async Task Public_ShowsOnlyPublicEventsInRange()
    {
        var owner = await factory.CreateUserAsync();
        await Create(owner, new EventBody("講座", Utc(2026, 5, 10), Utc(2026, 5, 10, 11)));
        await Create(owner, new EventBody("私人行程", Utc(2026, 5, 12), Utc(2026, 5, 12, 10), IsPublic: false));
        await Create(owner, new EventBody("範圍外", Utc(2026, 7, 1), Utc(2026, 7, 1, 10)));

        var occurrences = await PublicOccurrences(owner.Username, Utc(2026, 5, 1, 0), Utc(2026, 6, 1, 0));

        Assert.Equal(["講座"], occurrences.Select(o => o.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task Public_EventStartingBeforeRangeButEndingInside_IsIncluded()
    {
        var owner = await factory.CreateUserAsync();
        await Create(owner, new EventBody("跨月展覽", Utc(2026, 4, 25), Utc(2026, 5, 5)));

        var occurrences = await PublicOccurrences(owner.Username, Utc(2026, 5, 1, 0), Utc(2026, 6, 1, 0));

        Assert.Single(occurrences);
    }

    [Fact]
    public async Task Public_UnknownUser_IsNotFound()
    {
        var response = await factory.CreateClient().GetAsync($"/api/public/users/nobody-here/calendar{Range(Utc(2026, 1, 1), Utc(2026, 2, 1))}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?from=2026-01-01T00:00:00Z")]
    [InlineData("?from=2026-02-01T00:00:00Z&to=2026-01-01T00:00:00Z")]
    [InlineData("?from=2026-01-01T00:00:00Z&to=2027-06-01T00:00:00Z")]
    public async Task Public_MissingInvertedOrTooLongRange_IsBadRequest(string query)
    {
        var owner = await factory.CreateUserAsync();

        var response = await factory.CreateClient().GetAsync($"/api/public/users/{owner.Username}/calendar{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- recurrence ----------

    [Fact]
    public async Task WeeklyEvent_ExpandsIntoEachOccurrenceWithinRange()
    {
        var owner = await factory.CreateUserAsync();
        await Create(owner, new EventBody("每週課程", Utc(2026, 3, 2), Utc(2026, 3, 2, 10), Recurrence: "Weekly"));

        var starts = Starts(await PublicOccurrences(owner.Username, Utc(2026, 3, 10, 0), Utc(2026, 4, 1, 0)));

        Assert.Equal([Utc(2026, 3, 16), Utc(2026, 3, 23), Utc(2026, 3, 30)], starts);
    }

    [Fact]
    public async Task RecurringEvent_StopsAfterRecurrenceUntil()
    {
        var owner = await factory.CreateUserAsync();
        await Create(owner, new EventBody("短期每日", Utc(2026, 3, 1), Utc(2026, 3, 1, 10),
            Recurrence: "Daily", RecurrenceUntil: new DateOnly(2026, 3, 3)));

        var starts = Starts(await PublicOccurrences(owner.Username, Utc(2026, 3, 1, 0), Utc(2026, 4, 1, 0)));

        Assert.Equal([Utc(2026, 3, 1), Utc(2026, 3, 2), Utc(2026, 3, 3)], starts);
    }

    [Fact]
    public async Task MonthlyEventOnThe31st_FallsBackToMonthEndWithoutDrifting()
    {
        var owner = await factory.CreateUserAsync();
        await Create(owner, new EventBody("月底結算", Utc(2026, 1, 31), Utc(2026, 1, 31, 10), Recurrence: "Monthly"));

        var starts = Starts(await PublicOccurrences(owner.Username, Utc(2026, 1, 1, 0), Utc(2026, 4, 30, 0)));

        Assert.Equal([Utc(2026, 1, 31), Utc(2026, 2, 28), Utc(2026, 3, 31)], starts);
    }

    [Fact]
    public async Task Occurrences_ReferenceTheirEvent()
    {
        var owner = await factory.CreateUserAsync();
        var created = await Create(owner, new EventBody("每日站會", Utc(2026, 3, 2), Utc(2026, 3, 2, 10), Recurrence: "Daily"));

        var occurrences = await MyOccurrences(owner, Utc(2026, 3, 2, 0), Utc(2026, 3, 5, 0));

        Assert.All(occurrences, o => Assert.Equal(created.GetProperty("id").GetInt32(), o.GetProperty("eventId").GetInt32()));
        Assert.Equal(3, occurrences.Count);
    }

    // ---------- me ----------

    [Fact]
    public async Task Me_Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync($"/api/me/calendar{Range(Utc(2026, 1, 1), Utc(2026, 2, 1))}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_SeesOwnPrivateEventsButNotOtherUsers()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        await Create(me, new EventBody("私人看診", Utc(2026, 5, 3), Utc(2026, 5, 3, 10), IsPublic: false));
        await Create(other, new EventBody("別人的行程", Utc(2026, 5, 3), Utc(2026, 5, 3, 10)));

        var occurrences = await MyOccurrences(me, Utc(2026, 5, 1, 0), Utc(2026, 6, 1, 0));

        Assert.Equal(["私人看診"], occurrences.Select(o => o.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task Me_UpdateReadOrDeleteSomeoneElsesEvent_IsNotFound()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        var created = await Create(owner, new EventBody("原始", Utc(2026, 5, 3), Utc(2026, 5, 3, 10)));
        var url = $"/api/me/calendar/events/{created.GetProperty("id").GetInt32()}";

        var read = await intruder.Client().GetAsync(url);
        var update = await intruder.Client().PutJsonAsync(url, new EventBody("竄改", Utc(2026, 5, 3), Utc(2026, 5, 3, 10)));
        var delete = await intruder.Client().DeleteAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Me_UpdateAndDelete_OwnEvent()
    {
        var me = await factory.CreateUserAsync();
        var created = await Create(me, new EventBody("初稿", Utc(2026, 5, 3), Utc(2026, 5, 3, 10)));
        var url = $"/api/me/calendar/events/{created.GetProperty("id").GetInt32()}";

        var updated = await (await me.Client().PutJsonAsync(url, new EventBody("定稿", Utc(2026, 5, 4), Utc(2026, 5, 4, 10), Color: "#3b82f6")))
            .ReadDataAsync<JsonElement>();
        var delete = await me.Client().DeleteAsync(url);
        var remaining = await MyOccurrences(me, Utc(2026, 5, 1, 0), Utc(2026, 6, 1, 0));

        Assert.Equal(("定稿", "#3b82f6"), (updated.GetProperty("title").GetString(), updated.GetProperty("color").GetString()));
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task Me_EndBeforeStart_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync("/api/me/calendar/events", new EventBody("時間錯誤", Utc(2026, 5, 3, 10), Utc(2026, 5, 3, 9)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_InvalidColor_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync("/api/me/calendar/events",
            new EventBody("顏色錯誤", Utc(2026, 5, 3), Utc(2026, 5, 3, 10), Color: "red;background:url(x)"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
