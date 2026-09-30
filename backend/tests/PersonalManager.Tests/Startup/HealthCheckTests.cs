using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Startup;

public class HealthCheckTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_WithSqliteProvider_ReportsHealthy()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
    }
}
