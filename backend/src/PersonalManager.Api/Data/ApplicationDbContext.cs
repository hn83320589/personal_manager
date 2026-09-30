using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Data;

/// <summary>
/// 共用的資料模型。實際使用時透過 provider 專屬的子類別
/// （<see cref="SqliteApplicationDbContext"/>、<see cref="MySqlApplicationDbContext"/>）建立，
/// 讓兩種資料庫各自維護一組 migration。
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected ApplicationDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PersonalProfile> PersonalProfiles => Set<PersonalProfile>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<GuestBookEntry> GuestBookEntries => Set<GuestBookEntry>();
    public DbSet<ContactMethod> ContactMethods => Set<ContactMethod>();
    public DbSet<FileUpload> FileUploads => Set<FileUpload>();
    public DbSet<PortfolioAttachment> PortfolioAttachments => Set<PortfolioAttachment>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enum → string conversions (store as readable strings in DB)
        modelBuilder.Entity<Skill>()
            .Property(e => e.Level)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<BlogPost>()
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<WorkTask>()
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<WorkTask>()
            .Property(e => e.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<TodoItem>()
            .Property(e => e.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<TodoItem>()
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<ContactMethod>()
            .Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<PersonalProfile>(profile =>
        {
            profile.Property(e => e.PortfolioMode).HasConversion<string>().HasMaxLength(20);
            profile.Property(e => e.CardStyle).HasConversion<string>().HasMaxLength(20);
            profile.Property(e => e.CardRatio).HasConversion<string>().HasMaxLength(20);
            profile.Property(e => e.SkillDisplay).HasConversion<string>().HasMaxLength(20);
        });

        // WorkTask → Project FK (nullable, set null on delete)
        modelBuilder.Entity<WorkTask>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // BlogPost ↔ Tag many-to-many
        modelBuilder.Entity<BlogPost>()
            .HasMany(b => b.TagEntities)
            .WithMany()
            .UsingEntity("BlogPostTags");

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        ConfigureLookupIndexes(modelBuilder);
    }

    /// <summary>
    /// 查詢用索引。全部由 model 定義，才會出現在兩種 provider 的 migration 中
    /// （原本以 raw SQL 在啟動時建立，migration 看不到）。
    /// </summary>
    private static void ConfigureLookupIndexes(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(e => e.IsActive);

        b.Entity<PersonalProfile>().HasIndex(e => e.UserId).IsUnique();   // 每位使用者一份

        b.Entity<Education>().HasIndex(e => e.UserId);
        b.Entity<Education>().HasIndex(e => new { e.IsPublic, e.SortOrder });

        b.Entity<WorkExperience>().HasIndex(e => e.UserId);
        b.Entity<WorkExperience>().HasIndex(e => new { e.IsPublic, e.SortOrder });
        b.Entity<WorkExperience>().HasIndex(e => e.IsCurrent);
        b.Entity<WorkExperience>().HasIndex(e => new { e.StartDate, e.EndDate });

        b.Entity<Skill>().HasIndex(e => e.UserId);
        b.Entity<Skill>().HasIndex(e => e.Category);
        b.Entity<Skill>().HasIndex(e => new { e.IsPublic, e.SortOrder });

        b.Entity<Portfolio>().HasIndex(e => e.UserId);
        b.Entity<Portfolio>().HasIndex(e => new { e.IsPublic, e.IsFeatured });
        b.Entity<Portfolio>().HasIndex(e => e.SortOrder);

        b.Entity<CalendarEvent>().HasIndex(e => e.UserId);
        b.Entity<CalendarEvent>().HasIndex(e => new { e.StartTime, e.EndTime });
        b.Entity<CalendarEvent>().HasIndex(e => e.IsPublic);

        b.Entity<WorkTask>().HasIndex(e => e.UserId);
        b.Entity<WorkTask>().HasIndex(e => e.Status);
        b.Entity<WorkTask>().HasIndex(e => e.Priority);
        b.Entity<WorkTask>().HasIndex(e => e.DueDate);

        b.Entity<TodoItem>().HasIndex(e => e.UserId);
        b.Entity<TodoItem>().HasIndex(e => e.Status);
        b.Entity<TodoItem>().HasIndex(e => e.Priority);
        b.Entity<TodoItem>().HasIndex(e => e.DueDate);

        b.Entity<BlogPost>().HasIndex(e => e.UserId);
        b.Entity<BlogPost>().HasIndex(e => e.Status);
        b.Entity<BlogPost>().HasIndex(e => e.PublishedAt);
        b.Entity<BlogPost>().HasIndex(e => e.Category);
        b.Entity<BlogPost>().HasIndex(e => e.ViewCount);

        b.Entity<GuestBookEntry>().HasIndex(e => new { e.TargetUserId, e.IsApproved });
        b.Entity<GuestBookEntry>().HasIndex(e => e.CreatedAt);
        b.Entity<GuestBookEntry>().HasIndex(e => e.Email);

        b.Entity<ContactMethod>().HasIndex(e => e.UserId);
        b.Entity<ContactMethod>().HasIndex(e => e.Type);
        b.Entity<ContactMethod>().HasIndex(e => new { e.IsPublic, e.SortOrder });

        b.Entity<TimeEntry>().HasIndex(e => e.UserId);
        b.Entity<Project>().HasIndex(e => e.UserId);
        b.Entity<FileUpload>().HasIndex(e => e.UserId);

        // token 驗證時以 token 值查詢；值本身需唯一
        b.Entity<RefreshToken>().HasIndex(e => e.Token).IsUnique();
        b.Entity<PasswordResetToken>().Property(e => e.Token).HasMaxLength(256);
        b.Entity<PasswordResetToken>().HasIndex(e => e.Token).IsUnique();

        b.Entity<Tag>().HasIndex(e => new { e.UserId, e.Name }).IsUnique();
    }
}
