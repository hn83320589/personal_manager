using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

/// <summary>工作追蹤的專案：純後台的清單型資源。</summary>
public class ProjectsApiTests(ApiFactory factory) : OwnedCollectionContract(factory)
{
    protected override string Resource => "projects";

    protected override object NewItem(string label, bool isPublic = true) => new { name = label, color = "#3b82f6" };

    protected override string LabelOf(JsonElement item) => item.GetProperty("name").GetString()!;

    [Fact]
    public async Task InvalidColor_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl, new { name = "專案", color = "blue;}" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public class WorkTasksAndTimeEntriesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static Task<JsonElement> Post(TestUser user, string url, object body) => user.PostCreatedAsync(url, body);

    private static Task<JsonElement> CreateProject(TestUser user, string name) => Post(user, "/api/me/projects", new { name });

    private static Task<JsonElement> CreateTask(TestUser user, string title, int? projectId = null, double estimatedHours = 0) =>
        Post(user, "/api/me/work-tasks", new { title, projectId, estimatedHours, status = "InProgress", priority = "Medium" });

    private static async Task<List<JsonElement>> MyTasks(TestUser user, string query = "") =>
        await (await user.Client().GetAsync($"/api/me/work-tasks{query}")).ReadDataAsync<List<JsonElement>>();

    // ---------- work tasks ----------

    [Fact]
    public async Task Tasks_Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/work-tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tasks_ShowProjectNameAndCanBeFilteredByProject()
    {
        var me = await factory.CreateUserAsync();
        var branding = await CreateProject(me, "山茶行");
        await CreateTask(me, "標誌提案", branding.Id());
        await CreateTask(me, "報價單");

        var filtered = await MyTasks(me, $"?projectId={branding.Id()}");

        var task = Assert.Single(filtered);
        Assert.Equal(("標誌提案", "山茶行"), (task.GetProperty("title").GetString(), task.GetProperty("projectName").GetString()));
    }

    [Fact]
    public async Task Tasks_CannotBeAttachedToSomeoneElsesProject()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        var theirs = await CreateProject(other, "別人的專案");

        var response = await me.Client().PostJsonAsync("/api/me/work-tasks", new { title = "偷掛", projectId = theirs.Id() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Tasks_SomeoneElsesTask_IsNotFound()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        var task = await CreateTask(owner, "私人任務");

        var update = await intruder.Client().PutJsonAsync($"/api/me/work-tasks/{task.Id()}", new { title = "竄改" });
        var delete = await intruder.Client().DeleteAsync($"/api/me/work-tasks/{task.Id()}");

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Tasks_ActualTimeIsTheSumOfTheirTimeEntries()
    {
        var me = await factory.CreateUserAsync();
        var task = await CreateTask(me, "排版", estimatedHours: 3);
        await Post(me, "/api/me/time-entries", new { workTaskId = task.Id(), date = "2026-05-04", durationMinutes = 90 });
        await Post(me, "/api/me/time-entries", new { workTaskId = task.Id(), date = "2026-05-05", durationMinutes = 45 });

        var listed = (await MyTasks(me)).Single();

        Assert.Equal(135, listed.GetProperty("actualMinutes").GetInt32());
    }

    [Fact]
    public async Task DeletingAProject_KeepsItsTasksWithoutProject()
    {
        var me = await factory.CreateUserAsync();
        var project = await CreateProject(me, "結案專案");
        await CreateTask(me, "留下來的任務", project.Id());

        await me.Client().DeleteAsync($"/api/me/projects/{project.Id()}");
        var task = (await MyTasks(me)).Single();

        Assert.Equal(JsonValueKind.Null, task.GetProperty("projectId").ValueKind);
    }

    // ---------- time entries ----------

    [Fact]
    public async Task TimeEntry_WithStartAndEnd_CalculatesDuration()
    {
        var me = await factory.CreateUserAsync();

        var entry = await Post(me, "/api/me/time-entries",
            new { title = "客戶會議", date = "2026-05-04", startTime = "09:30", endTime = "11:00" });

        Assert.Equal(90, entry.GetProperty("durationMinutes").GetInt32());
    }

    [Fact]
    public async Task TimeEntry_EndBeforeStart_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync("/api/me/time-entries",
            new { title = "時間錯誤", date = "2026-05-04", startTime = "11:00", endTime = "09:00" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TimeEntry_WithoutDurationOrTimes_IsBadRequest()
    {
        var me = await factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync("/api/me/time-entries", new { title = "沒有時間", date = "2026-05-04" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TimeEntry_CannotBeLinkedToSomeoneElsesTask()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        var theirs = await CreateTask(other, "別人的任務");

        var response = await me.Client().PostJsonAsync("/api/me/time-entries",
            new { workTaskId = theirs.Id(), date = "2026-05-04", durationMinutes = 30 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TimeEntries_AreFilteredByDateRangeAndOwner()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        await Post(me, "/api/me/time-entries", new { title = "五月", date = "2026-05-10", durationMinutes = 60 });
        await Post(me, "/api/me/time-entries", new { title = "六月", date = "2026-06-10", durationMinutes = 60 });
        await Post(other, "/api/me/time-entries", new { title = "別人的", date = "2026-05-10", durationMinutes = 60 });

        var entries = await (await me.Client().GetAsync("/api/me/time-entries?from=2026-05-01&to=2026-05-31"))
            .ReadDataAsync<List<JsonElement>>();

        Assert.Equal(["五月"], entries.Select(e => e.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task TimeEntry_LinkedToTask_ShowsTaskAndProject()
    {
        var me = await factory.CreateUserAsync();
        var project = await CreateProject(me, "日光選物");
        var task = await CreateTask(me, "包裝打樣", project.Id());

        var entry = await Post(me, "/api/me/time-entries", new { workTaskId = task.Id(), date = "2026-05-04", durationMinutes = 30 });

        Assert.Equal(("包裝打樣", "日光選物"),
            (entry.GetProperty("workTaskTitle").GetString(), entry.GetProperty("projectName").GetString()));
    }

    [Fact]
    public async Task Summary_TotalsMinutesPerProjectWithinRange()
    {
        var me = await factory.CreateUserAsync();
        var project = await CreateProject(me, "山茶行");
        var task = await CreateTask(me, "標誌", project.Id());
        await Post(me, "/api/me/time-entries", new { workTaskId = task.Id(), date = "2026-05-04", durationMinutes = 120 });
        await Post(me, "/api/me/time-entries", new { workTaskId = task.Id(), date = "2026-05-05", durationMinutes = 60 });
        await Post(me, "/api/me/time-entries", new { title = "雜務", date = "2026-05-05", durationMinutes = 30 });

        var summary = await (await me.Client().GetAsync("/api/me/time-entries/summary?from=2026-05-01&to=2026-05-31"))
            .ReadDataAsync<JsonElement>();

        Assert.Equal(210, summary.GetProperty("totalMinutes").GetInt32());
        var byProject = summary.GetProperty("byProject").EnumerateArray()
            .ToDictionary(p => p.GetProperty("projectName").GetString() ?? "", p => p.GetProperty("minutes").GetInt32());
        Assert.Equal(180, byProject["山茶行"]);
        Assert.Equal(30, byProject[""]);
    }
}
