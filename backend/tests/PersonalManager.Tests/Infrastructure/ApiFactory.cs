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
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"personal-manager-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
