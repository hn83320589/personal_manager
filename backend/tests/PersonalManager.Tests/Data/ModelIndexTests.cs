using Microsoft.EntityFrameworkCore.Metadata;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Tests.Data;

/// <summary>
/// 索引由 EF model 管理（會出現在 migration 中），兩種 provider 的模型都要有。
/// </summary>
public class ModelIndexTests
{
    public static TheoryData<string> Providers => new() { "Sqlite", "MySql" };

    private static IModel BuildModel(string provider)
    {
        ApplicationDbContext context = provider == "Sqlite"
            ? new SqliteDesignTimeFactory().CreateDbContext([])
            : new MySqlDesignTimeFactory().CreateDbContext([]);
        using (context)
            return context.Model;
    }

    private static IIndex? FindIndex(IModel model, Type entity, params string[] columns) =>
        model.FindEntityType(entity)!.GetIndexes()
            .FirstOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(columns));

    [Theory]
    [MemberData(nameof(Providers))]
    public void RefreshTokenLookup_IsUniqueIndexed(string provider)
    {
        var index = FindIndex(BuildModel(provider), typeof(RefreshToken), nameof(RefreshToken.Token));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void PasswordResetTokenLookup_IsUniqueIndexedWithBoundedLength(string provider)
    {
        var model = BuildModel(provider);
        var index = FindIndex(model, typeof(PasswordResetToken), nameof(PasswordResetToken.Token));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
        // MySQL 無法對沒有長度上限的 longtext 欄位建立索引
        Assert.NotNull(model.FindEntityType(typeof(PasswordResetToken))!
            .FindProperty(nameof(PasswordResetToken.Token))!.GetMaxLength());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void TagName_IsUniquePerUser(string provider)
    {
        var index = FindIndex(BuildModel(provider), typeof(Tag), nameof(Tag.UserId), nameof(Tag.Name));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void OwnedEntities_AreIndexedByUserId(string provider)
    {
        var model = BuildModel(provider);
        Type[] owned =
        [
            typeof(PersonalProfile), typeof(Education), typeof(WorkExperience), typeof(Skill), typeof(Portfolio),
            typeof(CalendarEvent), typeof(TodoItem), typeof(WorkTask), typeof(BlogPost), typeof(ContactMethod),
            typeof(TimeEntry), typeof(Project), typeof(FileUpload)
        ];

        var missing = owned.Where(t => model.FindEntityType(t)!.GetIndexes()
            .All(i => i.Properties[0].Name != "UserId")).Select(t => t.Name).ToList();

        Assert.Empty(missing);
    }
}
