using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace PersonalManager.Tests.Infrastructure;

/// <summary>
/// 以獨立的暫存 SQLite 檔案啟動整個 API，讓整合測試走真實的 HTTP 與資料庫路徑。
/// 每個 factory 實例使用自己的資料庫檔，測試類別之間互不影響。
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSecret = "integration-test-signing-key-0123456789abcdef";

    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"personal-manager-test-{Guid.NewGuid():N}.db");
    private readonly string _environment;
    private readonly string? _jwtSecret;
    private readonly IReadOnlyDictionary<string, string?> _settings;

    /// <summary>xUnit fixture 使用的預設設定：Testing 環境 + 合法的測試用簽章金鑰。</summary>
    public ApiFactory() : this("Testing", TestJwtSecret, null) { }

    /// <summary>自訂環境與金鑰，用於測試啟動時的設定驗證。</summary>
    /// <param name="jwtSecret">null 表示不覆寫，沿用 appsettings.json 的值。</param>
    /// <param name="settings">額外覆寫的設定值，例如 <c>Cors:AllowedOrigins:0</c>。</param>
    public static ApiFactory For(
        string environment, string? jwtSecret = TestJwtSecret, IReadOnlyDictionary<string, string?>? settings = null)
        => new(environment, jwtSecret, settings);

    private ApiFactory(string environment, string? jwtSecret, IReadOnlyDictionary<string, string?>? settings)
    {
        _environment = environment;
        _jwtSecret = jwtSecret;
        _settings = settings ?? new Dictionary<string, string?>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");
        // 測試都從同一個「IP」發出，放寬限流以免互相影響；限流本身由專門的測試以較低額度驗證
        builder.UseSetting("RateLimiting:AuthPermitsPerMinute", "10000");
        builder.UseSetting("RateLimiting:PublicWritePermitsPerMinute", "10000");
        if (_jwtSecret is not null)
            builder.UseSetting("Jwt:SecretKey", _jwtSecret);
        foreach (var (key, value) in _settings)
            builder.UseSetting(key, value);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
