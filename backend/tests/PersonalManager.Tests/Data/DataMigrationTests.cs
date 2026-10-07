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

    [Fact]
    public async Task HashedAuthTokens_ClearsLegacyPlaintextTokens()
    {
        await MigrateToMigrationBeforeAsync("HashedAuthTokens");
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, IsActive, CreatedAt, UpdatedAt)
            VALUES ('legacy', 'legacy@test.local', 'x', 'Legacy', 'User', 1, '2026-01-01', '2026-01-01')
            """);
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, IsRevoked, CreatedAt)
            VALUES (1, 'plain-1', '2030-01-01', 0, '2026-01-01'), (1, 'plain-2', '2030-01-01', 0, '2026-01-01')
            """);

        await _db.Database.MigrateAsync();
        var remaining = await _db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM RefreshTokens").SingleAsync();

        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task BlockPortfolio_KeepsLegacyPortfoliosReadableWithUniqueSlugs()
    {
        await MigrateToMigrationBeforeAsync("BlockPortfolio");
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, IsActive, CreatedAt, UpdatedAt)
            VALUES ('legacy', 'legacy@test.local', 'x', 'Legacy', 'User', 1, '2026-01-01', '2026-01-01')
            """);
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO Portfolios (UserId, Title, Description, ImageUrl, ProjectUrl, RepositoryUrl, Technologies,
                                    IsFeatured, IsPublic, SortOrder, CreatedAt, UpdatedAt)
            VALUES (1, '舊作品一', '舊的描述', '', '', '', 'Vue', 0, 1, 1, '2026-01-01', '2026-01-01'),
                   (1, '舊作品二', '', '', '', '', '', 0, 1, 2, '2026-01-01', '2026-01-01')
            """);

        await _db.Database.MigrateAsync();
        var portfolios = await _db.Portfolios.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

        Assert.Equal(("舊的描述", 2, 0), (portfolios[0].Summary, portfolios.Select(p => p.Slug).Distinct().Count(), portfolios[0].Blocks.Count));
    }

    [Fact]
    public async Task RemoveContactMethodIcon_KeepsExistingContacts()
    {
        await MigrateToMigrationBeforeAsync("RemoveContactMethodIcon");
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, IsActive, CreatedAt, UpdatedAt)
            VALUES ('legacy', 'legacy@test.local', 'x', 'Legacy', 'User', 1, '2026-01-01', '2026-01-01')
            """);
        await _db.Database.ExecuteSqlAsync($"""
            INSERT INTO ContactMethods (UserId, Type, Label, Value, Icon, IsPublic, SortOrder, CreatedAt, UpdatedAt)
            VALUES (1, 'Email', '工作信箱', 'me@example.com', 'email', 1, 1, '2026-01-01', '2026-01-01')
            """);

        await _db.Database.MigrateAsync();
        var value = await _db.Database.SqlQuery<string>($"SELECT Value FROM ContactMethods").SingleAsync();

        Assert.Equal("me@example.com", value);
    }

    public void Dispose()
    {
        _db.Dispose();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }
}
