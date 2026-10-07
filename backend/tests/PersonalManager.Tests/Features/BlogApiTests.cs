using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class BlogApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private sealed record PostBody(
        string Title, string? Content = "<p>內文</p>", string? Slug = null, string? Summary = null,
        string? Category = null, string[]? Tags = null, string? CoverImageUrl = null,
        string Status = "Published", DateTime? PublishedAt = null);

    private static Task<JsonElement> Create(TestUser user, PostBody body) => user.PostCreatedAsync("/api/me/posts", body);

    private static string Slug(JsonElement post) => post.GetProperty("slug").GetString()!;

    private async Task<List<string>> PublicTitles(string username, string query = "")
    {
        var page = await (await factory.CreateClient().GetAsync($"/api/public/users/{username}/posts{query}"))
            .ReadDataAsync<Paged<JsonElement>>();
        return page.Items.Select(p => p.GetProperty("title").GetString()!).ToList();
    }

    private async Task<string> SavedContent(TestUser user, string html)
    {
        var post = await Create(user, new PostBody("清洗測試", Content: html));
        return post.GetProperty("content").GetString()!;
    }

    // ---------- public visibility ----------

    [Fact]
    public async Task PublicList_ShowsOnlyPublishedPostsNewestFirst()
    {
        var author = await factory.CreateUserAsync();
        await Create(author, new PostBody("較早", PublishedAt: DateTime.UtcNow.AddDays(-2)));
        await Create(author, new PostBody("草稿", Status: "Draft"));
        await Create(author, new PostBody("封存", Status: "Archived"));
        await Create(author, new PostBody("排程中", PublishedAt: DateTime.UtcNow.AddDays(3)));
        await Create(author, new PostBody("較新", PublishedAt: DateTime.UtcNow.AddDays(-1)));

        Assert.Equal(["較新", "較早"], await PublicTitles(author.Username));
    }

    [Fact]
    public async Task PublicDetail_DraftOrScheduledPost_IsNotFound()
    {
        var author = await factory.CreateUserAsync();
        var draft = await Create(author, new PostBody("草稿", Status: "Draft"));
        var scheduled = await Create(author, new PostBody("排程", PublishedAt: DateTime.UtcNow.AddDays(1)));
        var client = factory.CreateClient();

        var draftResponse = await client.GetAsync($"/api/public/users/{author.Username}/posts/{Slug(draft)}");
        var scheduledResponse = await client.GetAsync($"/api/public/users/{author.Username}/posts/{Slug(scheduled)}");

        Assert.Equal(HttpStatusCode.NotFound, draftResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, scheduledResponse.StatusCode);
    }

    [Fact]
    public async Task PublicDetail_PublishedPost_IncludesContent()
    {
        var author = await factory.CreateUserAsync();
        var post = await Create(author, new PostBody("品牌改造筆記", Content: "<p>顏色是記憶的錨點</p>"));

        var detail = await (await factory.CreateClient().GetAsync($"/api/public/users/{author.Username}/posts/{Slug(post)}"))
            .ReadDataAsync<JsonElement>();

        Assert.Contains("顏色是記憶的錨點", detail.GetProperty("content").GetString());
    }

    [Fact]
    public async Task PublicList_FiltersByKeywordTagAndCategoryAndPages()
    {
        var author = await factory.CreateUserAsync();
        await Create(author, new PostBody("兩色印刷的技巧", Category: "印刷", Tags: ["包裝"]));
        await Create(author, new PostBody("接案報價", Category: "接案", Tags: ["接案"]));
        await Create(author, new PostBody("品牌改造", Category: "設計筆記", Tags: ["品牌", "包裝"]));

        Assert.Equal(["兩色印刷的技巧"], await PublicTitles(author.Username, "?q=印刷"));
        Assert.Equal(2, (await PublicTitles(author.Username, "?tag=包裝")).Count);
        Assert.Equal(["接案報價"], await PublicTitles(author.Username, "?category=接案"));
        Assert.Single(await PublicTitles(author.Username, "?page=2&pageSize=2"));
    }

    [Fact]
    public async Task PublicFacets_ListCategoriesAndTagsOfPublishedPostsOnly()
    {
        var author = await factory.CreateUserAsync();
        await Create(author, new PostBody("A", Category: "設計", Tags: ["品牌"]));
        await Create(author, new PostBody("B", Category: "秘密分類", Tags: ["秘密標籤"], Status: "Draft"));

        var facets = await (await factory.CreateClient().GetAsync($"/api/public/users/{author.Username}/posts/facets"))
            .ReadDataAsync<JsonElement>();

        Assert.Equal(["設計"], facets.GetProperty("categories").EnumerateArray().Select(c => c.GetString()));
        Assert.Equal(["品牌"], facets.GetProperty("tags").EnumerateArray().Select(t => t.GetString()));
    }

    [Fact]
    public async Task RecordingAView_IncrementsViewCount()
    {
        var author = await factory.CreateUserAsync();
        var post = await Create(author, new PostBody("熱門文章"));
        var client = factory.CreateClient();

        await client.PostAsync($"/api/public/users/{author.Username}/posts/{Slug(post)}/views", null);
        await client.PostAsync($"/api/public/users/{author.Username}/posts/{Slug(post)}/views", null);
        var mine = await (await author.Client().GetAsync($"/api/me/posts/{post.Id()}")).ReadDataAsync<JsonElement>();

        Assert.Equal(2, mine.GetProperty("viewCount").GetInt32());
    }

    // ---------- slugs ----------

    [Fact]
    public async Task Slug_IsGeneratedFromLatinTitleAndMadeUniquePerUser()
    {
        var author = await factory.CreateUserAsync();

        var first = await Create(author, new PostBody("Offline First PWA!"));
        var second = await Create(author, new PostBody("Offline first PWA"));

        Assert.Equal("offline-first-pwa", Slug(first));
        Assert.Equal("offline-first-pwa-2", Slug(second));
    }

    [Fact]
    public async Task Slug_ForTitleWithoutLatinCharacters_FallsBackToGeneratedValue()
    {
        var author = await factory.CreateUserAsync();

        var post = await Create(author, new PostBody("品牌改造時，老顧客最在意的是什麼"));

        Assert.Matches("^post-[a-z0-9]+$", Slug(post));
    }

    [Fact]
    public async Task Slug_SameSlugForDifferentUsers_IsAllowedAndResolvedPerUser()
    {
        var alice = await factory.CreateUserAsync();
        var bob = await factory.CreateUserAsync();
        await Create(alice, new PostBody("Alice 的文章", Slug: "hello"));
        await Create(bob, new PostBody("Bob 的文章", Slug: "hello"));

        var alicePost = await (await factory.CreateClient().GetAsync($"/api/public/users/{alice.Username}/posts/hello"))
            .ReadDataAsync<JsonElement>();

        Assert.Equal("Alice 的文章", alicePost.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Slug_WithInvalidCharacters_IsBadRequest()
    {
        var author = await factory.CreateUserAsync();

        var response = await author.Client().PostJsonAsync("/api/me/posts", new PostBody("標題", Slug: "has spaces/and?"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- content sanitising ----------

    [Theory]
    [InlineData("<p>安全</p><script>alert(1)</script>", "<script")]
    [InlineData("<p onclick=\"alert(1)\">點我</p>", "onclick")]
    [InlineData("<a href=\"javascript:alert(1)\">連結</a>", "javascript:")]
    [InlineData("<iframe src=\"https://evil.example\"></iframe>", "<iframe")]
    [InlineData("<img src=\"x\" onerror=\"alert(1)\">", "onerror")]
    [InlineData("<div data-embed=\"https://evil.example/player\"></div>", "evil.example")]
    public async Task Content_IsSanitisedBeforeSaving(string html, string mustNotContain)
    {
        var author = await factory.CreateUserAsync();

        var saved = await SavedContent(author, html);

        Assert.DoesNotContain(mustNotContain, saved, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("<figure><img src=\"/files/a.jpg\" alt=\"示意\"><figcaption>圖片說明</figcaption></figure>", "<figcaption>圖片說明</figcaption>")]
    [InlineData("<div data-embed=\"https://www.youtube.com/watch?v=abc\"></div>", "data-embed=\"https://www.youtube.com/watch?v=abc\"")]
    [InlineData("<pre><code class=\"language-csharp\">var x = 1;</code></pre>", "class=\"language-csharp\"")]
    [InlineData("<p style=\"text-align: center\">置中</p>", "text-align: center")]
    [InlineData("<h2>小標</h2><ul><li>項目</li></ul><blockquote>引言</blockquote>", "<blockquote>引言</blockquote>")]
    public async Task Content_KeepsEditorFormatting(string html, string mustContain)
    {
        var author = await factory.CreateUserAsync();

        var saved = await SavedContent(author, html);

        Assert.Contains(mustContain, saved);
    }

    // ---------- me ----------

    [Fact]
    public async Task Me_Anonymous_IsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/posts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_List_IncludesDraftsButNotOtherUsersPosts()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        await Create(me, new PostBody("我的草稿", Status: "Draft"));
        await Create(other, new PostBody("別人的文章"));

        var page = await (await me.Client().GetAsync("/api/me/posts")).ReadDataAsync<Paged<JsonElement>>();

        Assert.Equal(["我的草稿"], page.Items.Select(p => p.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task Me_CoverImageIsSaved()
    {
        var me = await factory.CreateUserAsync();

        var post = await Create(me, new PostBody("有封面", CoverImageUrl: "/files/cover.jpg"));

        Assert.Equal("/files/cover.jpg", post.GetProperty("coverImageUrl").GetString());
    }

    [Fact]
    public async Task Me_PublishingWithoutDate_SetsPublishedAtToNow()
    {
        var me = await factory.CreateUserAsync();

        var post = await Create(me, new PostBody("立即發佈"));

        var publishedAt = post.GetProperty("publishedAt").GetDateTime();
        Assert.InRange(publishedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task Me_ReadingMinutesAreCalculated()
    {
        var me = await factory.CreateUserAsync();

        var post = await Create(me, new PostBody("長文", Content: $"<p>{new string('字', 2000)}</p>"));

        Assert.Equal(5, post.GetProperty("readingMinutes").GetInt32());
    }

    [Fact]
    public async Task Me_TagsAreNormalisedAndOfferedForAutocomplete()
    {
        var me = await factory.CreateUserAsync();

        await Create(me, new PostBody("A", Tags: [" 品牌 ", "包裝", "品牌"]));
        await Create(me, new PostBody("B", Tags: ["包裝", "印刷"]));
        var tags = await (await me.Client().GetAsync("/api/me/tags")).ReadDataAsync<List<string>>();

        Assert.Equal(["包裝", "印刷", "品牌"], tags.Order());
    }

    [Fact]
    public async Task Me_UpdateOrDeleteSomeoneElsesPost_IsNotFound()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        var post = await Create(owner, new PostBody("原文"));

        var update = await intruder.Client().PutJsonAsync($"/api/me/posts/{post.Id()}", new PostBody("被竄改"));
        var delete = await intruder.Client().DeleteAsync($"/api/me/posts/{post.Id()}");
        var read = await intruder.Client().GetAsync($"/api/me/posts/{post.Id()}");

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
    }

    [Fact]
    public async Task Me_Update_ChangesPostAndKeepsSlugUnlessChanged()
    {
        var me = await factory.CreateUserAsync();
        var post = await Create(me, new PostBody("Original title"));

        var updated = await (await me.Client().PutJsonAsync($"/api/me/posts/{post.Id()}",
            new PostBody("New title", Slug: Slug(post), Status: "Draft"))).ReadDataAsync<JsonElement>();

        Assert.Equal(("New title", "original-title", "Draft"),
            (updated.GetProperty("title").GetString(), Slug(updated), updated.GetProperty("status").GetString()));
    }
}
