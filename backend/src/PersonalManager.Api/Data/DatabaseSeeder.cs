using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // 已有資料就跳過
        if (await db.Users.AnyAsync()) return;

        // ── 使用者 ──────────────────────────────────────────────────
        var admin = new User
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FullName = "管理員",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var john = new User
        {
            Username = "john_doe",
            Email = "john.doe@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FullName = "John Doe",
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.AddRange(admin, john);
        await db.SaveChangesAsync();

        // ── 個人資料 ─────────────────────────────────────────────────
        db.PersonalProfiles.Add(new PersonalProfile
        {
            UserId = admin.Id,
            Title = "全端開發工程師",
            Summary = "專精於 .NET Core 與 Vue.js 開發",
            Description = "擁有5年以上全端開發經驗，熟悉現代化網頁開發技術棧，包括後端 API 設計、前端 SPA 開發與雲端部署。",
            ProfileImageUrl = "",
            Website = "https://example.com",
            Location = "台灣 台北",
            ThemeColor = "blue",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ── 學歷 ─────────────────────────────────────────────────────
        db.Educations.AddRange(
            new Education
            {
                UserId = admin.Id,
                School = "國立台灣大學",
                Degree = "碩士",
                FieldOfStudy = "資訊工程學系",
                StartYear = 2015,
                EndYear = 2017,
                Description = "主修軟體工程與資料庫系統",
                IsPublic = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Education
            {
                UserId = admin.Id,
                School = "國立交通大學",
                Degree = "學士",
                FieldOfStudy = "資訊工程學系",
                StartYear = 2011,
                EndYear = 2015,
                Description = "主修程式設計與演算法",
                IsPublic = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // ── 工作經歷 ─────────────────────────────────────────────────
        db.WorkExperiences.AddRange(
            new WorkExperience
            {
                UserId = admin.Id,
                Company = "ABC科技公司",
                Position = "資深全端工程師",
                StartDate = new DateOnly(2020, 3, 1),
                IsCurrent = true,
                Description = "負責企業級應用系統開發與維護，主導多項核心功能設計與重構",
                IsPublic = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new WorkExperience
            {
                UserId = admin.Id,
                Company = "XYZ軟體公司",
                Position = "後端工程師",
                StartDate = new DateOnly(2017, 7, 1),
                EndDate = new DateOnly(2020, 2, 28),
                IsCurrent = false,
                Description = "開發 RESTful API 與資料庫設計，負責系統效能優化",
                IsPublic = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // ── 技能 ─────────────────────────────────────────────────────
        db.Skills.AddRange(
            new Skill { UserId = admin.Id, Name = "C# .NET Core", Category = "後端開發", Level = SkillLevel.Expert, YearsOfExperience = 5, IsPublic = true, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Skill { UserId = admin.Id, Name = "Vue.js", Category = "前端開發", Level = SkillLevel.Advanced, YearsOfExperience = 3, IsPublic = true, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Skill { UserId = admin.Id, Name = "MariaDB/MySQL", Category = "資料庫", Level = SkillLevel.Advanced, YearsOfExperience = 4, IsPublic = true, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Skill { UserId = admin.Id, Name = "Docker", Category = "DevOps", Level = SkillLevel.Intermediate, YearsOfExperience = 2, IsPublic = true, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // ── 作品集 ───────────────────────────────────────────────────
        db.Portfolios.AddRange(
            new Portfolio
            {
                UserId = admin.Id,
                Title = "Personal Manager 系統",
                Description = "個人服務管理系統，包含履歷、待辦事項、行事曆等功能",
                ImageUrl = "",
                Technologies = ".NET Core, Vue.js, MariaDB",
                ProjectUrl = "https://personalmanager.example.com",
                RepositoryUrl = "https://github.com/example/personal-manager",
                IsFeatured = true,
                IsPublic = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Portfolio
            {
                UserId = admin.Id,
                Title = "E-Commerce API",
                Description = "電商平台後端 API 系統",
                ImageUrl = "",
                Technologies = ".NET Core, Entity Framework, Redis",
                RepositoryUrl = "https://github.com/example/ecommerce-api",
                IsFeatured = false,
                IsPublic = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // ── 行事曆 ───────────────────────────────────────────────────
        db.CalendarEvents.AddRange(
            new CalendarEvent
            {
                UserId = admin.Id,
                Title = "專案會議",
                Description = "討論 Personal Manager 系統需求",
                StartTime = new DateTime(2025, 8, 15, 10, 0, 0),
                EndTime = new DateTime(2025, 8, 15, 11, 30, 0),
                IsAllDay = false,
                IsPublic = false,
                Color = "#3B82F6",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new CalendarEvent
            {
                UserId = admin.Id,
                Title = "技術研討會",
                Description = "參加 .NET Conf 2025",
                StartTime = new DateTime(2025, 9, 1, 9, 0, 0),
                EndTime = new DateTime(2025, 9, 1, 17, 0, 0),
                IsAllDay = true,
                IsPublic = true,
                Color = "#10B981",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // ── 工作任務 ─────────────────────────────────────────────────
        db.WorkTasks.AddRange(
            new WorkTask
            {
                UserId = admin.Id,
                Title = "完成資料庫設計",
                Description = "設計 Personal Manager 系統的資料庫結構",
                Status = WorkTaskStatus.InProgress,
                Priority = WorkTaskPriority.High,
                DueDate = new DateTime(2025, 8, 13, 18, 0, 0),
                CompletedAt = null,
                EstimatedHours = 8.0,
                ActualHours = 0.0,
                Tags = "資料庫,設計,架構",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new WorkTask
            {
                UserId = admin.Id,
                Title = "開發使用者認證 API",
                Description = "實作 JWT Token 認證機制",
                Status = WorkTaskStatus.Planning,
                Priority = WorkTaskPriority.High,
                DueDate = new DateTime(2025, 8, 16, 18, 0, 0),
                CompletedAt = null,
                EstimatedHours = 16.0,
                ActualHours = 0.0,
                Tags = "認證,JWT,安全性",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // ── 待辦事項 ─────────────────────────────────────────────────
        db.TodoItems.AddRange(
            new TodoItem { UserId = admin.Id, Title = "完成資料庫設計文檔", Description = "撰寫詳細的資料庫設計說明文檔", Priority = TodoPriority.High, Status = TodoStatus.Pending, DueDate = new DateTime(2025, 8, 13, 18, 0, 0), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TodoItem { UserId = admin.Id, Title = "購買開發用伺服器", Description = "評估並購買適合的雲端伺服器方案", Priority = TodoPriority.Medium, Status = TodoStatus.Pending, DueDate = new DateTime(2025, 8, 20, 12, 0, 0), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TodoItem { UserId = admin.Id, Title = "學習新技術", Description = "研讀 Docker Kubernetes 相關文檔", Priority = TodoPriority.Low, Status = TodoStatus.Pending, DueDate = new DateTime(2025, 8, 30, 23, 59, 0), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // ── 部落格 ───────────────────────────────────────────────────
        db.BlogPosts.AddRange(
            new BlogPost
            {
                UserId = admin.Id,
                Title = "如何設計可擴展的資料庫架構",
                Slug = "how-to-design-scalable-database-architecture",
                Content = "在現代應用程式開發中，資料庫設計是非常重要的一環...",
                Summary = "分享資料庫架構設計的經驗與最佳實踐",
                Status = BlogPostStatus.Published,
                ViewCount = 0,
                PublishedAt = new DateTime(2025, 8, 10, 10, 0, 0),
                Category = "技術分享",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new BlogPost
            {
                UserId = admin.Id,
                Title = "Vue.js 3 與 TypeScript 開發心得",
                Slug = "vuejs3-typescript-development-experience",
                Content = "Vue.js 3 結合 TypeScript 可以大幅提升開發效率...",
                Summary = "分享 Vue3 + TypeScript 的開發經驗",
                Status = BlogPostStatus.Draft,
                ViewCount = 0,
                Category = "前端開發",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // ── 留言板 ───────────────────────────────────────────────────
        db.GuestBookEntries.AddRange(
            new GuestBookEntry { TargetUserId = admin.Id, Name = "訪客A", Email = "visitor@example.com", Message = "很棒的個人網站！期待看到更多技術分享。", IsApproved = true, AdminReply = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new GuestBookEntry { TargetUserId = admin.Id, Name = "John Smith", Email = "john@example.com", Message = "感謝分享這些實用的開發經驗，對我很有幫助！", IsApproved = true, AdminReply = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // ── 聯絡方式 ─────────────────────────────────────────────────
        db.ContactMethods.AddRange(
            new ContactMethod { UserId = admin.Id, Type = ContactType.Email, Value = "admin@example.com", Label = "工作信箱", Icon = "email", IsPublic = true, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ContactMethod { UserId = admin.Id, Type = ContactType.Phone, Value = "0912345678", Label = "手機", Icon = "phone", IsPublic = true, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ContactMethod { UserId = admin.Id, Type = ContactType.GitHub, Value = "https://github.com/example", Label = "GitHub", Icon = "github", IsPublic = true, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ContactMethod { UserId = admin.Id, Type = ContactType.LinkedIn, Value = "https://linkedin.com/in/example", Label = "LinkedIn", Icon = "linkedin", IsPublic = true, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();
    }
}
