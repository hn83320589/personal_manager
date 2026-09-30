using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalManager.Api.Data;

namespace PersonalManager.Tests.Data;

/// <summary>會轉換既有資料的 migration：先遷移到前一版並寫入舊格式的資料，再遷移到最新版檢查結果。</summary>
public sealed class DataMigrationTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"personal-manager-migration-{Guid.NewGuid():N}.db");
    private readonly SqliteApplicationDbContext _db;

    public DataMigrationTests()
    {
        var options = new DbContextOptionsBuilder<SqliteApplicationDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        _db = new SqliteApplicationDbContext(options);
    }

    private async Task MigrateToMigrationBeforeAsync(string migrationName)
    {
        var migrations = _db.Database.GetMigrations().ToList();
        var previous = migrations[migrations.FindIndex(m => m.EndsWith("_" + migrationName)) - 1];
        await _db.GetService<IMigrator>().MigrateAsync(previous);
    }

    [Theory]
    [InlineData("image", "Image")]
    [InlineData("pdf", "Pdf")]
    [InlineData("document", "Word")]
    [InlineData("presentation", "PowerPoint")]
    [InlineData("other", "Archive")]
    public async Task FileKindAndDimensions_ConvertsLegacyFileType(string legacyType, string expectedKind)
    {
        await MigrateToMigrationBeforeAsync("FileKindAndDimensions");
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, IsActive, CreatedAt, UpdatedAt)
            VALUES ('legacy', 'legacy@test.local', 'x', 'Legacy', 'User', 1, '2026-01-01', '2026-01-01')
            """);
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO FileUploads (UserId, FileName, StoredName, FileUrl, FileType, FileSize, MimeType, CreatedAt)
            VALUES (1, 'old-file', 'stored', '/files/stored', {legacyType}, 10, 'x', '2026-01-01')
            """);

        await _db.Database.MigrateAsync();
        var kind = await _db.Database.SqlQuery<string>($"SELECT Kind AS Value FROM FileUploads").SingleAsync();

        Assert.Equal(expectedKind, kind);
    }

    public void Dispose()
    {
        _db.Dispose();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
