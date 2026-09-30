using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersonalManager.Api.Data;

// 每個 provider 一個子類別，只為了讓 EF 把 migration 分開存放（Migrations/Sqlite、Migrations/MySql）。
// 模型定義全部在 ApplicationDbContext。

public class SqliteApplicationDbContext : ApplicationDbContext
{
    public SqliteApplicationDbContext(DbContextOptions<SqliteApplicationDbContext> options) : base(options) { }
}

public class MySqlApplicationDbContext : ApplicationDbContext
{
    public MySqlApplicationDbContext(DbContextOptions<MySqlApplicationDbContext> options) : base(options) { }
}

/// <summary>`dotnet ef migrations add X --context SqliteApplicationDbContext` 使用。</summary>
public class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<SqliteApplicationDbContext>
{
    public SqliteApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqliteApplicationDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;
        return new SqliteApplicationDbContext(options);
    }
}

/// <summary>
/// `dotnet ef migrations add X --context MySqlApplicationDbContext` 使用。
/// 以固定的 server version 產生 migration，不需要連線到實際的資料庫。
/// </summary>
public class MySqlDesignTimeFactory : IDesignTimeDbContextFactory<MySqlApplicationDbContext>
{
    public MySqlApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MySqlApplicationDbContext>()
            .UseMySql("Server=localhost;Database=personal_manager", PersistenceSetup.MySqlServerVersion)
            .Options;
        return new MySqlApplicationDbContext(options);
    }
}
