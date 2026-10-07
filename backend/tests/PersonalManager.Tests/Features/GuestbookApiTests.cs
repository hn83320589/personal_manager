using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class GuestbookApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static string PublicUrl(string username) => $"/api/public/users/{username}/guestbook";

    private async Task<HttpResponseMessage> Leave(string username, string message, string name = "訪客", string? email = "guest@example.com") =>
        await factory.CreateClient().PostJsonAsync(PublicUrl(username), new { name, email, message });

    private async Task<List<JsonElement>> PublicEntries(string username) =>
        (await (await factory.CreateClient().GetAsync(PublicUrl(username))).ReadDataAsync<Paged<JsonElement>>()).Items;

    private static async Task<List<JsonElement>> MyEntries(TestUser owner, string query = "") =>
        (await (await owner.Client().GetAsync($"/api/me/guestbook{query}")).ReadDataAsync<Paged<JsonElement>>()).Items;

    private static string Message(JsonElement entry) => entry.GetProperty("message").GetString()!;

    // ---------- visitors ----------

    [Fact]
    public async Task Leaving_A_Message_IsAcceptedButHiddenUntilApproved()
    {
        var owner = await factory.CreateUserAsync();

        var response = await Leave(owner.Username, "作品很棒！");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Empty(await PublicEntries(owner.Username));
    }

    [Fact]
    public async Task PublicList_ShowsApprovedEntriesWithoutEmail()
    {
        var owner = await factory.CreateUserAsync();
        await Leave(owner.Username, "第一則", email: "secret-visitor@example.com");
        var pending = (await MyEntries(owner)).Single();
        await owner.Client().PutJsonAsync($"/api/me/guestbook/{pending.Id()}/approval", new { isApproved = true });

        var response = await factory.CreateClient().GetAsync(PublicUrl(owner.Username));
        var json = await response.Content.ReadAsStringAsync();
        var entries = (await response.ReadDataAsync<Paged<JsonElement>>()).Items;

        Assert.Equal(["第一則"], entries.Select(Message));
        Assert.DoesNotContain("secret-visitor@example.com", json);
        Assert.DoesNotContain("\"email\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Leaving_A_Message_ForUnknownUser_IsNotFound()
    {
        var response = await Leave("nobody-here", "哈囉");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("", "內容")]
    [InlineData("訪客", "")]
    public async Task Leaving_A_Message_WithoutNameOrMessage_IsBadRequest(string name, string message)
    {
        var owner = await factory.CreateUserAsync();

        var response = await Leave(owner.Username, message, name);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Leaving_A_Message_WithInvalidEmail_IsBadRequest()
    {
        var owner = await factory.CreateUserAsync();

        var response = await Leave(owner.Username, "哈囉", email: "not-an-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Leaving_A_Message_EmailIsOptional()
    {
        var owner = await factory.CreateUserAsync();

        var response = await Leave(owner.Username, "不留 Email", email: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // ---------- owner ----------

    [Fact]
    public async Task Me_Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/guestbook");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_List_ShowsEntriesLeftForMeIncludingPendingAndEmail()
    {
        var owner = await factory.CreateUserAsync();
        var someoneElse = await factory.CreateUserAsync();
        await Leave(owner.Username, "給我的", email: "fan@example.com");
        await Leave(someoneElse.Username, "給別人的");

        var entries = await MyEntries(owner);

        var entry = Assert.Single(entries);
        Assert.Equal(("給我的", "fan@example.com", false),
            (Message(entry), entry.GetProperty("email").GetString(), entry.GetProperty("isApproved").GetBoolean()));
    }

    [Fact]
    public async Task Me_List_CanFilterPendingEntries()
    {
        var owner = await factory.CreateUserAsync();
        await Leave(owner.Username, "待審核");
        await Leave(owner.Username, "已審核");
        var approved = (await MyEntries(owner)).Single(e => Message(e) == "已審核");
        await owner.Client().PutJsonAsync($"/api/me/guestbook/{approved.Id()}/approval", new { isApproved = true });

        var pending = await MyEntries(owner, "?status=pending");

        Assert.Equal(["待審核"], pending.Select(Message));
    }

    [Fact]
    public async Task Me_Reply_IsShownPublicly()
    {
        var owner = await factory.CreateUserAsync();
        await Leave(owner.Username, "請問有接案嗎？");
        var entry = (await MyEntries(owner)).Single();
        await owner.Client().PutJsonAsync($"/api/me/guestbook/{entry.Id()}/approval", new { isApproved = true });

        var reply = await owner.Client().PutJsonAsync($"/api/me/guestbook/{entry.Id()}/reply", new { reply = "有的，歡迎來信" });
        var shown = (await PublicEntries(owner.Username)).Single();

        Assert.Equal(HttpStatusCode.OK, reply.StatusCode);
        Assert.Equal("有的，歡迎來信", shown.GetProperty("reply").GetString());
    }

    [Fact]
    public async Task Me_ManagingSomeoneElsesGuestbook_IsNotFound()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        await Leave(owner.Username, "給版主");
        var entry = (await MyEntries(owner)).Single();

        var approve = await intruder.Client().PutJsonAsync($"/api/me/guestbook/{entry.Id()}/approval", new { isApproved = true });
        var reply = await intruder.Client().PutJsonAsync($"/api/me/guestbook/{entry.Id()}/reply", new { reply = "冒充回覆" });
        var delete = await intruder.Client().DeleteAsync($"/api/me/guestbook/{entry.Id()}");

        Assert.Equal(HttpStatusCode.NotFound, approve.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, reply.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Single(await MyEntries(owner));
    }

    [Fact]
    public async Task Me_Delete_RemovesEntry()
    {
        var owner = await factory.CreateUserAsync();
        await Leave(owner.Username, "垃圾留言");
        var entry = (await MyEntries(owner)).Single();

        var response = await owner.Client().DeleteAsync($"/api/me/guestbook/{entry.Id()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await MyEntries(owner));
    }
}

public class GuestbookRateLimitTests
{
    [Fact]
    public async Task LeavingTooManyMessages_IsRejectedWith429()
    {
        using var factory = ApiFactory.For("Testing", settings: new Dictionary<string, string?>
        {
            ["RateLimiting:PublicWritePermitsPerMinute"] = "2"
        });
        var owner = await factory.CreateUserAsync();
        var client = factory.CreateClient();
        var body = new { name = "訪客", message = "洗版" };

        await client.PostJsonAsync($"/api/public/users/{owner.Username}/guestbook", body);
        await client.PostJsonAsync($"/api/public/users/{owner.Username}/guestbook", body);
        var third = await client.PostJsonAsync($"/api/public/users/{owner.Username}/guestbook", body);

        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }
}
