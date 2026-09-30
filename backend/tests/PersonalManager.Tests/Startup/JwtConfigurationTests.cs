using System.Net;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Startup;

public class JwtConfigurationTests
{
    [Fact]
    public void Startup_OutsideDevelopment_WithPlaceholderSecret_RefusesToStart()
    {
        // 不覆寫 Jwt:SecretKey，沿用 appsettings.json 的空字串
        using var factory = ApiFactory.For("Production", jwtSecret: null);

        var error = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(error);
        Assert.Contains("Jwt:SecretKey", FlattenMessages(error));
    }

    [Fact]
    public void Startup_OutsideDevelopment_WithShortSecret_RefusesToStart()
    {
        using var factory = ApiFactory.For("Production", jwtSecret: "too-short");

        var error = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(error);
        Assert.Contains("Jwt:SecretKey", FlattenMessages(error));
    }

    [Fact]
    public async Task Startup_InDevelopment_WithoutSecret_StartsWithGeneratedKey()
    {
        using var factory = ApiFactory.For("Development", jwtSecret: null);

        var response = await factory.CreateClient().GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string FlattenMessages(Exception error)
    {
        var messages = new List<string>();
        for (var current = error; current is not null; current = current.InnerException)
            messages.Add(current.Message);
        return string.Join(" | ", messages);
    }
}
