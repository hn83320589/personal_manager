using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class PortfoliosApiTests(ApiFactory factory) : PublicCollectionContract(factory)
{
    protected override string Resource => "portfolios";

    protected override object NewItem(string label, bool isPublic = true) => new { title = label, isPublic };

    protected override string LabelOf(JsonElement item) => item.GetProperty("title").GetString()!;

    private static int Id(JsonElement item) => item.GetProperty("id").GetInt32();

    private async Task<JsonElement> Save(TestUser user, int id, object document)
    {
        var response = await user.Client().PutJsonAsync($"{MeUrl}/{id}", document);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadDataAsync<JsonElement>();
    }

    private async Task<HttpResponseMessage> TrySave(TestUser user, object document)
    {
        var created = await CreateAsync(user, NewItem("作品"));
        return await user.Client().PutJsonAsync($"{MeUrl}/{Id(created)}", document);
    }

    private async Task<JsonElement> PublicDetail(string username, string slug) =>
        await (await Factory.CreateClient().GetAsync($"{PublicUrl(username)}/{slug}")).ReadDataAsync<JsonElement>();

    /// <summary>一份完整的作品：包含所有區塊類型、自訂欄位、連結與封面。</summary>
    private static object FullDocument(int coverFileId, int galleryFileId, int attachmentFileId) => new
    {
        title = "山茶行 品牌識別",
        slug = "shancha",
        summary = "為經營四十年的茶行重新整理品牌。",
        category = "品牌識別",
        year = 2025,
        role = "品牌識別設計",
        period = "2024.10 – 2025.04",
        tags = new[] { "品牌", "包裝" },
        isFeatured = true,
        isPublic = true,
        coverFocus = "50% 30%",
        covers = new object[] { new { fileId = coverFileId }, new { url = "https://images.example.com/second.jpg" } },
        fields = new[] { new { label = "客戶", value = "山茶行" }, new { label = "媒材", value = "紙本、燙金" } },
        links = new[] { new { label = "Behance", url = "https://behance.net/example" } },
        blocks = new object[]
        {
            new { type = "text", title = "委託內容", html = "<p>保留老顧客記得的紅色。</p>" },
            new { type = "image", layout = "bleed", image = new { fileId = coverFileId, caption = "主標誌", alt = "山茶行標誌" } },
            new { type = "gallery", layout = "masonry", items = new[] { new { fileId = galleryFileId, caption = "名片", alt = "名片設計" } } },
            new { type = "files", title = "相關文件", items = new[] { new { fileId = attachmentFileId, description = "品牌規範手冊" } } },
            new { type = "embed", url = "https://vimeo.com/123456", caption = "動態版" },
            new { type = "metrics", title = "成果", items = new[] { new { value = "+38%", label = "回購率" }, new { value = "3 週", label = "上市時間" } } },
            new { type = "code", language = "css", code = ".logo { color: #9e2b35; }", caption = "品牌色" }
        }
    };

    private async Task<(TestUser Owner, JsonElement Saved)> SaveFullDocument()
    {
        var owner = await Factory.CreateUserAsync();
        var cover = await owner.UploadAsync("cover.png", SampleFiles.Png(1600, 1200));
        var gallery = await owner.UploadAsync("card.jpg", SampleFiles.Jpeg(800, 1000));
        var guide = await owner.UploadAsync("guide.pdf", SampleFiles.Pdf());
        var created = await CreateAsync(owner, NewItem("草稿"));
        var saved = await Save(owner, Id(created), FullDocument(Id(cover), Id(gallery), Id(guide)));
        return (owner, saved);
    }

    // ---------- document round trip ----------

    [Fact]
    public async Task Create_WithOnlyTitle_GeneratesSlugAndEmptyDocument()
    {
        var me = await Factory.CreateUserAsync();

        var created = await CreateAsync(me, new { title = "Trailnote Hiking App" });

        Assert.Equal("trailnote-hiking-app", created.GetProperty("slug").GetString());
        Assert.Equal(0, created.GetProperty("blocks").GetArrayLength());
    }

    [Fact]
    public async Task Save_FullDocument_RoundTripsEveryBlockType()
    {
        var (_, saved) = await SaveFullDocument();

        var types = saved.GetProperty("blocks").EnumerateArray().Select(b => b.GetProperty("type").GetString());
        Assert.Equal(["text", "image", "gallery", "files", "embed", "metrics", "code"], types);
        Assert.Equal(("品牌識別設計", "2024.10 – 2025.04"), (saved.GetProperty("role").GetString(), saved.GetProperty("period").GetString()));
        Assert.Equal(["客戶", "媒材"], saved.GetProperty("fields").EnumerateArray().Select(f => f.GetProperty("label").GetString()));
        Assert.Equal(["包裝", "品牌"], saved.GetProperty("tags").EnumerateArray().Select(t => t.GetString()).Order());
    }

    [Fact]
    public async Task UploadedImages_AreResolvedToUrlAndDimensionsByTheServer()
    {
        var (_, saved) = await SaveFullDocument();

        var cover = saved.GetProperty("covers")[0];
        var galleryItem = saved.GetProperty("blocks")[2].GetProperty("items")[0];
        Assert.StartsWith("/files/", cover.GetProperty("url").GetString());
        Assert.Equal((1600, 1200), (cover.GetProperty("width").GetInt32(), cover.GetProperty("height").GetInt32()));
        Assert.Equal((800, 1000), (galleryItem.GetProperty("width").GetInt32(), galleryItem.GetProperty("height").GetInt32()));
        Assert.Equal("名片", galleryItem.GetProperty("caption").GetString());
    }

    [Fact]
    public async Task Attachments_AreResolvedToFileNameKindAndSizeByTheServer()
    {
        var (_, saved) = await SaveFullDocument();

        var attachment = saved.GetProperty("blocks")[3].GetProperty("items")[0];
        Assert.Equal(("guide.pdf", "Pdf", "品牌規範手冊"), (
            attachment.GetProperty("fileName").GetString(), attachment.GetProperty("kind").GetString(),
            attachment.GetProperty("description").GetString()));
        Assert.True(attachment.GetProperty("size").GetInt64() > 0);
    }

    [Fact]
    public async Task PublicDetail_ShowsTheWholeCaseStudy()
    {
        var (owner, _) = await SaveFullDocument();

        var detail = await PublicDetail(owner.Username, "shancha");

        Assert.Equal("山茶行 品牌識別", detail.GetProperty("title").GetString());
        Assert.Equal(7, detail.GetProperty("blocks").GetArrayLength());
        Assert.Equal("Behance", detail.GetProperty("links")[0].GetProperty("label").GetString());
    }

    [Fact]
    public async Task PublicCards_IncludeCoversAndHighlightMetrics()
    {
        var (owner, _) = await SaveFullDocument();

        var card = (await (await Factory.CreateClient().GetAsync(PublicUrl(owner.Username))).ReadDataAsync<List<JsonElement>>()).Single();

        Assert.Equal(2, card.GetProperty("covers").GetArrayLength());
        Assert.Equal("50% 30%", card.GetProperty("coverFocus").GetString());
        Assert.Equal(["+38%", "3 週"], card.GetProperty("metrics").EnumerateArray().Select(m => m.GetProperty("value").GetString()));
        Assert.False(card.TryGetProperty("blocks", out _));
    }

    [Fact]
    public async Task PublicDetail_PrivatePortfolio_IsNotFound()
    {
        var owner = await Factory.CreateUserAsync();
        var created = await CreateAsync(owner, new { title = "Secret Work", isPublic = false });

        var response = await Factory.CreateClient().GetAsync($"{PublicUrl(owner.Username)}/{created.GetProperty("slug").GetString()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PublicList_FiltersByCategoryAndTag_AndFacetsListThem()
    {
        var owner = await Factory.CreateUserAsync();
        var poster = await CreateAsync(owner, NewItem("海報"));
        var package = await CreateAsync(owner, NewItem("包裝"));
        await Save(owner, Id(poster), new { title = "海報", category = "海報與主視覺", tags = new[] { "印刷" }, isPublic = true });
        await Save(owner, Id(package), new { title = "包裝", category = "包裝", tags = new[] { "印刷", "包裝" }, isPublic = true });

        var byCategory = await (await Factory.CreateClient().GetAsync($"{PublicUrl(owner.Username)}?category=包裝")).ReadDataAsync<List<JsonElement>>();
        var byTag = await (await Factory.CreateClient().GetAsync($"{PublicUrl(owner.Username)}?tag=印刷")).ReadDataAsync<List<JsonElement>>();
        var facets = await (await Factory.CreateClient().GetAsync($"{PublicUrl(owner.Username)}/facets")).ReadDataAsync<JsonElement>();

        Assert.Equal(["包裝"], byCategory.Select(LabelOf));
        Assert.Equal(2, byTag.Count);
        Assert.Equal(["包裝", "海報與主視覺"], facets.GetProperty("categories").EnumerateArray().Select(c => c.GetString()).Order());
    }

    // ---------- validation & security ----------

    [Fact]
    public async Task TextBlockHtml_IsSanitised()
    {
        var me = await Factory.CreateUserAsync();
        var created = await CreateAsync(me, NewItem("作品"));

        var saved = await Save(me, Id(created), new
        {
            title = "作品",
            blocks = new object[] { new { type = "text", html = "<p onclick=\"alert(1)\">安全</p><script>alert(1)</script>" } }
        });

        var html = saved.GetProperty("blocks")[0].GetProperty("html").GetString()!;
        Assert.DoesNotContain("<script", html);
        Assert.DoesNotContain("onclick", html);
        Assert.Contains("安全", html);
    }

    [Fact]
    public async Task EmbedFromUnlistedSite_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await TrySave(me, new
        {
            title = "作品",
            blocks = new object[] { new { type = "embed", url = "https://evil.example/player" } }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UsingSomeoneElsesUploadedFile_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();
        var other = await Factory.CreateUserAsync();
        var theirs = await other.UploadAsync("theirs.png", SampleFiles.Png(10, 10));

        var response = await TrySave(me, new { title = "作品", covers = new[] { new { fileId = Id(theirs) } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UsingADocumentAsAnImage_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();
        var pdf = await me.UploadAsync("guide.pdf", SampleFiles.Pdf());

        var response = await TrySave(me, new
        {
            title = "作品",
            blocks = new object[] { new { type = "image", image = new { fileId = Id(pdf) } } }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("http://insecure.example/image.jpg")]
    public async Task ExternalImageThatIsNotHttps_IsBadRequest(string url)
    {
        var me = await Factory.CreateUserAsync();

        var response = await TrySave(me, new { title = "作品", covers = new[] { new { url } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LinkWithScriptUrl_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await TrySave(me, new { title = "作品", links = new[] { new { label = "點我", url = "javascript:alert(1)" } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidCoverFocus_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await TrySave(me, new { title = "作品", coverFocus = "center; background: url(x)" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TooManyMetrics_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();
        var metrics = Enumerable.Range(1, 9).Select(i => new { value = $"{i}", label = "項目" }).ToArray();

        var response = await TrySave(me, new { title = "作品", blocks = new object[] { new { type = "metrics", items = metrics } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnknownBlockType_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();

        var response = await TrySave(me, new { title = "作品", blocks = new object[] { new { type = "html", raw = "<iframe>" } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SlugIsUniquePerUser()
    {
        var me = await Factory.CreateUserAsync();

        var first = await CreateAsync(me, new { title = "Poster" });
        var second = await CreateAsync(me, new { title = "Poster" });

        Assert.Equal(("poster", "poster-2"), (first.GetProperty("slug").GetString(), second.GetProperty("slug").GetString()));
    }
}
