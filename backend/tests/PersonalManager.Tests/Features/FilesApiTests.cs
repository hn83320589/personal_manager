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

    private static async Task<JsonElement> UploadOk(TestUser user, string fileName, byte[] content)
    {
        var response = await Upload(user.Client(), fileName, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadDataAsync<JsonElement>();
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

        var file = await UploadOk(me, "cover.png", SampleFiles.Png(1600, 900));

        Assert.Equal(("Image", 1600, 900, "image/png"), (
            file.GetProperty("kind").GetString(), file.GetProperty("width").GetInt32(),
            file.GetProperty("height").GetInt32(), file.GetProperty("mimeType").GetString()));
    }

    [Fact]
    public async Task UploadedFile_IsServedWithNoSniffHeader()
    {
        var me = await factory.CreateUserAsync();
        var file = await UploadOk(me, "cover.png", SampleFiles.Png(10, 10));

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
        await UploadOk(me, "photo.png", SampleFiles.Png(2, 2));
        await UploadOk(me, "guide.pdf", SampleFiles.Pdf());
        await UploadOk(other, "theirs.png", SampleFiles.Png(2, 2));

        var all = await MyFiles(me);
        var images = await MyFiles(me, "?kind=Image");

        Assert.Equal(2, all.Count);
        Assert.Equal(["photo.png"], images.Select(f => f.GetProperty("fileName").GetString()));
    }

    [Fact]
    public async Task Delete_RemovesOwnFileFromStorage()
    {
        var me = await factory.CreateUserAsync();
        var file = await UploadOk(me, "old.png", SampleFiles.Png(2, 2));

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
        var file = await UploadOk(owner, "mine.png", SampleFiles.Png(2, 2));

        var response = await intruder.Client().DeleteAsync($"/api/me/files/{file.GetProperty("id").GetInt32()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single(await MyFiles(owner));
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
