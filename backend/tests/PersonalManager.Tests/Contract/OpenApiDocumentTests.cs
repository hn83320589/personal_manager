using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using PersonalManager.Tests.Infrastructure;
using Swashbuckle.AspNetCore.Swagger;

namespace PersonalManager.Tests.Contract;

/// <summary>
/// 前端的 API 型別由 <c>backend/openapi.json</c> 產生（<c>npm run api:types</c>）。
/// 這個測試確保提交的文件與 API 一致：API 有變動時測試失敗，
/// 以 <c>UPDATE_OPENAPI=1 dotnet test --filter OpenApiDocument</c> 更新文件後再重新產生前端型別。
/// </summary>
public class OpenApiDocumentTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string DocumentPath = Path.Combine(FindBackendDirectory(), "openapi.json");

    [Fact]
    public void CommittedOpenApiDocument_MatchesTheApi()
    {
        var swagger = factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        var current = Normalize(swagger.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));

        if (Environment.GetEnvironmentVariable("UPDATE_OPENAPI") == "1")
            File.WriteAllText(DocumentPath, current);

        var committed = File.Exists(DocumentPath) ? Normalize(File.ReadAllText(DocumentPath)) : "";
        Assert.True(committed == current,
            "backend/openapi.json 與 API 不一致。執行 UPDATE_OPENAPI=1 dotnet test --filter OpenApiDocument 更新，" +
            "再到 frontend 執行 npm run api:types。");
    }

    private static string Normalize(string json) => json.Replace("\r\n", "\n").TrimEnd() + "\n";

    /// <summary>從測試執行目錄往上找到含有 PersonalManager.sln 的 backend 目錄。</summary>
    private static string FindBackendDirectory()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "PersonalManager.sln")))
                return dir.FullName;
        throw new InvalidOperationException("找不到 PersonalManager.sln");
    }
}
