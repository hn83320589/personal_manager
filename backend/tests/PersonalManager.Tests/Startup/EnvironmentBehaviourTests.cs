using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Startup;

/// <summary>只適合開發環境的行為（示範資料、Swagger、localhost CORS）不能出現在其他環境。</summary>
public class EnvironmentBehaviourTests
{
    [Fact]
    public async Task DemoData_OutsideDevelopment_IsNotSeeded()
    {
        using var factory = ApiFactory.For("Production");

        var users = await GetPublicUsers(factory.CreateClient());

        Assert.Equal(0, users.GetArrayLength());
    }

    [Fact]
    public async Task DemoData_InDevelopment_IsSeeded()
    {
        using var factory = ApiFactory.For("Development");

        var users = await GetPublicUsers(factory.CreateClient());

        Assert.True(users.GetArrayLength() > 0);
    }

    [Fact]
    public async Task Swagger_OutsideDevelopment_IsNotExposed()
    {
        using var factory = ApiFactory.For("Production");

        var response = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_InDevelopment_IsExposed()
    {
        using var factory = ApiFactory.For("Development");

        var response = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cors_AllowsOnlyConfiguredOrigins()
    {
        using var factory = ApiFactory.For("Production", settings: new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://portfolio.example.com"
        });
        var client = factory.CreateClient();

        var allowed = await Preflight(client, "https://portfolio.example.com");
        // 開發用的 localhost 來源不能因為設定陣列依索引合併而殘留到正式環境
        var deniedDevServer = await Preflight(client, "http://localhost:5173");
        var deniedPreview = await Preflight(client, "http://localhost:4173");

        Assert.Equal("https://portfolio.example.com",
            allowed.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(deniedDevServer.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(deniedPreview.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_InDevelopmentWithoutConfiguration_AllowsLocalFrontend()
    {
        using var factory = ApiFactory.For("Development");

        var response = await Preflight(factory.CreateClient(), "http://localhost:5173");

        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    private static async Task<JsonElement> GetPublicUsers(HttpClient client)
    {
        var body = JsonDocument.Parse(await client.GetStringAsync("/api/public/users"));
        return body.RootElement.GetProperty("data").GetProperty("items").Clone();
    }

    private static Task<HttpResponseMessage> Preflight(HttpClient client, string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/health");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        return client.SendAsync(request);
    }
}
