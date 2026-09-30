using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace PersonalManager.Api.Data;

public static class PersistenceSetup
{
    public const string SqliteProvider = "Sqlite";
    public const string MySqlProvider = "MySql";

    /// <summary>MySQL migration 以此版本產生；切換到實際的 MariaDB 時若版本差異大再調整。</summary>
    public static readonly ServerVersion MySqlServerVersion = new MariaDbServerVersion(new Version(10, 11, 0));

    private const string DefaultSqliteDatabase = "App_Data/personal_manager.db";

    /// <summary>
    /// 依 <c>Database:Provider</c> 註冊 DbContext。未設定時使用 SQLite；
    /// 設定值無法辨識時直接中止啟動，避免在錯誤的資料庫上執行。
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var provider = configuration["Database:Provider"] ?? SqliteProvider;
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (provider.Equals(SqliteProvider, StringComparison.OrdinalIgnoreCase))
        {
            var sqliteConnection = ResolveSqliteConnectionString(connectionString, environment.ContentRootPath);
            services.AddDbContext<ApplicationDbContext, SqliteApplicationDbContext>(
                options => options.UseSqlite(sqliteConnection));
        }
        else if (provider.Equals(MySqlProvider, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Database:Provider 為 MySql，但 ConnectionStrings:DefaultConnection 未設定。");
            services.AddDbContext<ApplicationDbContext, MySqlApplicationDbContext>(
                options => options.UseMySql(connectionString, MySqlServerVersion));
        }
        else
        {
            throw new InvalidOperationException(
                $"不支援的 Database:Provider「{provider}」，請使用 {SqliteProvider} 或 {MySqlProvider}。");
        }

        return services;
    }

    /// <summary>
    /// 未設定連線字串時使用 App_Data 下的預設檔案；相對路徑一律以專案根目錄為基準，
    /// 避免因為從不同目錄執行 <c>dotnet run</c> 而產生多個資料庫檔。
    /// </summary>
    private static string ResolveSqliteConnectionString(string? configured, string contentRoot)
    {
        var builder = new SqliteConnectionStringBuilder(
            string.IsNullOrWhiteSpace(configured) ? $"Data Source={DefaultSqliteDatabase}" : configured);

        var isFile = !string.IsNullOrEmpty(builder.DataSource)
                     && builder.DataSource != ":memory:"
                     && builder.Mode != SqliteOpenMode.Memory;
        if (isFile && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(contentRoot, builder.DataSource);
            Directory.CreateDirectory(Path.GetDirectoryName(builder.DataSource)!);
        }

        return builder.ToString();
    }
}
