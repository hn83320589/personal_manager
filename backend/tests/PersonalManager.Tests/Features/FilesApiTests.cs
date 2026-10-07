using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class FilesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static Task<HttpResponseMessage> Upload(HttpClient client, string fileName, byte[] content, string contentType = "application/octet-stream")
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var form = new MultipartFormDataContent { { file, "file", fileName } };
        return client.PostAsync("/api/me/files", form);
    }


    private static async Task<List<JsonElement>> MyFiles(TestUser user, string query = "") =>
        (await (await user.Client().GetAsync($"/api/me/files{query}")).ReadDataAsync<JsonElement>())
        .GetProperty("items").EnumerateArray().ToList();

    [Fact]
    public async Task Anonymous_IsUnauthorized()
    {
        var response = await Upload(factory.CreateClient(), "a.png", SampleFiles.Png(1, 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadingAnImage_RecordsKindDimensionsAndServerDecidedMimeType()
    {
        var me = await factory.CreateUserAsync();

        var file = await me.UploadAsync("cover.png", SampleFiles.Png(1600, 900));

        Assert.Equal(("Image", 1600, 900, "image/png"), (
            file.GetProperty("kind").GetString(), file.GetProperty("width").GetInt32(),
            file.GetProperty("height").GetInt32(), file.GetProperty("mimeType").GetString()));
    }

    [Fact]
    public async Task UploadedFile_IsServedWithNoSniffHeader()
    {
        var me = await factory.CreateUserAsync();
        var file = await me.UploadAsync("cover.png", SampleFiles.Png(10, 10));

        var served = await factory.CreateClient().GetAsync(file.GetProperty("url").GetString());

        Assert.Equal(HttpStatusCode.OK, served.StatusCode);
        Assert.Equal("nosniff", served.Headers.GetValues("X-Content-Type-Options").Single());
    }

    [Theory]
    [InlineData("disguised.png")]
    [InlineData("disguised.pdf")]
    public async Task FileWhoseContentDoesNotMatchItsExtension_IsRejected(string fileName)
    {
        var me = await factory.CreateUserAsync();

        var response = await Upload(me.Client(), fileName, SampleFiles.Html(), "image/png");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("vector.svg")]
    [InlineData("page.html")]
    [InlineData("source.psd")]
    public async Task UnsupportedFileType_IsRejected(string fileName)
    {
        var me = await factory.CreateUserAsync();

        var response = await Upload(me.Client(), fileName, SampleFiles.Png(1, 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnFilesAndCanFilterByKind()
    {
        var me = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        await me.UploadAsync("photo.png", SampleFiles.Png(2, 2));
        await me.UploadAsync("guide.pdf", SampleFiles.Pdf());
        await other.UploadAsync("theirs.png", SampleFiles.Png(2, 2));

        var all = await MyFiles(me);
        var images = await MyFiles(me, "?kind=Image");

        Assert.Equal(2, all.Count);
        Assert.Equal(["photo.png"], images.Select(f => f.GetProperty("fileName").GetString()));
    }

    [Fact]
    public async Task Delete_RemovesOwnFileFromStorage()
    {
        var me = await factory.CreateUserAsync();
        var file = await me.UploadAsync("old.png", SampleFiles.Png(2, 2));

        var response = await me.Client().DeleteAsync($"/api/me/files/{file.GetProperty("id").GetInt32()}");
        var served = await factory.CreateClient().GetAsync(file.GetProperty("url").GetString());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, served.StatusCode);
    }

    [Fact]
    public async Task DeletingSomeoneElsesFile_IsNotFound()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        var file = await owner.UploadAsync("mine.png", SampleFiles.Png(2, 2));

        var response = await intruder.Client().DeleteAsync($"/api/me/files/{file.GetProperty("id").GetInt32()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single(await MyFiles(owner));
    }

    // ---------- 檔案被哪些內容使用 ----------

    private static async Task<List<JsonElement>> Usages(TestUser user, int fileId) =>
        await (await user.Client().GetAsync($"/api/me/files/{fileId}/usages")).ReadDataAsync<List<JsonElement>>();

    private static string Describe(JsonElement usage) =>
        $"{usage.GetProperty("kind").GetString()}:{usage.GetProperty("title").GetString()}";

    [Fact]
    public async Task Usages_ListWorksPostsAndProfileThatUseTheFile()
    {
        var me = await factory.CreateUserAsync();
        var image = await me.UploadAsync("poster.png", SampleFiles.Png(10, 10));
        var (id, url) = (image.GetProperty("id").GetInt32(), image.GetProperty("url").GetString()!);
        var work = await (await me.Client().PostJsonAsync("/api/me/portfolios", new { title = "海報" })).ReadDataAsync<JsonElement>();
        await me.Client().PutJsonAsync($"/api/me/portfolios/{work.GetProperty("id").GetInt32()}", new
        {
            title = "海報",
            blocks = new object[] { new { type = "gallery", items = new[] { new { fileId = id } } } },
        });
        await me.Client().PostJsonAsync("/api/me/posts", new { title = "設計筆記", status = "Draft", coverImageUrl = url });
        await me.Client().PutJsonAsync("/api/me/profile", new
        {
            fullName = "Dada", themeColor = "blue", profileImageUrl = url,
            portfolioMode = "Designer", cardStyle = "Visual", cardRatio = "Portrait", skillDisplay = "NameOnly",
        });

        var usages = await Usages(me, id);

        Assert.Equal(["Portfolio:海報", "Post:設計筆記", "Profile:個人資料"], usages.Select(Describe).Order());
    }

    [Fact]
    public async Task Usages_FindImagesInsidePostContent()
    {
        var me = await factory.CreateUserAsync();
        var image = await me.UploadAsync("inline.png", SampleFiles.Png(10, 10));
        var url = image.GetProperty("url").GetString()!;
        await me.Client().PostJsonAsync("/api/me/posts", new
        {
            title = "有圖的文章", status = "Draft", content = $"<figure><img src=\"{url}\" alt=\"\"><figcaption></figcaption></figure>",
        });

        var usages = await Usages(me, image.GetProperty("id").GetInt32());

        Assert.Equal(["Post:有圖的文章"], usages.Select(Describe));
    }

    [Fact]
    public async Task Usages_OfAnUnusedFile_IsEmpty()
    {
        var me = await factory.CreateUserAsync();
        var file = await me.UploadAsync("spare.png", SampleFiles.Png(2, 2));

        Assert.Empty(await Usages(me, file.GetProperty("id").GetInt32()));
    }

    [Fact]
    public async Task UsagesOfSomeoneElsesFile_IsNotFound()
    {
        var owner = await factory.CreateUserAsync();
        var intruder = await factory.CreateUserAsync();
        var file = await owner.UploadAsync("mine.png", SampleFiles.Png(2, 2));

        var response = await intruder.Client().GetAsync($"/api/me/files/{file.GetProperty("id").GetInt32()}/usages");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class FileSizeLimitTests
{
    [Fact]
    public async Task FileLargerThanConfiguredLimit_IsRejected()
    {
        using var factory = ApiFactory.For("Testing", settings: new Dictionary<string, string?>
        {
            ["FileStorage:MaxFileSizeMB"] = "1"
        });
        var me = await factory.CreateUserAsync();
        var content = new byte[(int)(1.5 * 1024 * 1024)];
        SampleFiles.Pdf().CopyTo(content, 0);
        var form = new MultipartFormDataContent { { new ByteArrayContent(content), "file", "big.pdf" } };

        var response = await me.Client().PostAsync("/api/me/files", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
