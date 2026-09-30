using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class TodosApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private sealed record TodoBody(string Title, string Status = "Pending", string Priority = "Medium",
        string? Description = null, DateTime? DueDate = null);

    private static async Task<JsonElement> Create(TestUser user, TodoBody body)
    {
        var response = await user.Client().PostJsonAsync("/api/me/todos", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadDataAsync<JsonElement>();
    }

    private static async Task<List<string>> Titles(TestUser user, string query = "") =>
        (await (await user.Client().GetAsync($"/api/me/todos{query}")).ReadDataAsync<List<JsonElement>>())
        .Select(t => t.GetProperty("title").GetString()!).ToList();

    private static string Url(JsonElement todo) => $"/api/me/todos/{todo.GetProperty("id").GetInt32()}";

    [Fact]
    public async Task Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/todos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnTodos()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        await Create(me, new TodoBody("回覆客戶"));
        await Create(other, new TodoBody("別人的待辦"));

        Assert.Equal(["回覆客戶"], await Titles(me));
    }

    [Fact]
    public async Task List_CanFilterByStatus()
    {
        var me = await factory.CreateUserAsync();
        await Create(me, new TodoBody("進行中", Status: "InProgress"));
        await Create(me, new TodoBody("已完成", Status: "Completed"));

        Assert.Equal(["已完成"], await Titles(me, "?status=Completed"));
    }

    [Fact]
    public async Task Completing_SetsCompletedAt_AndReopeningClearsIt()
    {
        var me = await factory.CreateUserAsync();
        var todo = await Create(me, new TodoBody("寄出提案"));

        var completed = await (await me.Client().PutJsonAsync(Url(todo), new TodoBody("寄出提案", Status: "Completed")))
            .ReadDataAsync<JsonElement>();
        var reopened = await (await me.Client().PutJsonAsync(Url(todo), new TodoBody("寄出提案", Status: "Pending")))
            .ReadDataAsync<JsonElement>();

        Assert.NotEqual(JsonValueKind.Null, completed.GetProperty("completedAt").ValueKind);
        Assert.Equal(JsonValueKind.Null, reopened.GetProperty("completedAt").ValueKind);
    }

    [Fact]
    public async Task SomeoneElsesTodo_IsNotFound()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        var todo = await Create(owner, new TodoBody("私人待辦"));

        var update = await intruder.Client().PutJsonAsync(Url(todo), new TodoBody("竄改"));
        var delete = await intruder.Client().DeleteAsync(Url(todo));

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal(["私人待辦"], await Titles(owner));
    }

    [Fact]
    public async Task Delete_RemovesOwnTodo()
    {
        var me = await factory.CreateUserAsync();
        var todo = await Create(me, new TodoBody("刪掉我"));

        await me.Client().DeleteAsync(Url(todo));

        Assert.Empty(await Titles(me));
    }

    [Fact]
    public async Task WithoutTitle_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync("/api/me/todos", new TodoBody(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
