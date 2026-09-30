using PersonalManager.Api.Auth;
using PersonalManager.Api.Common;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Setup;

/// <summary>業務 service 的註冊。Phase 2 各 feature 重建後，改由各自的 feature 註冊。</summary>
public static class ApplicationSetup
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<RichTextSanitizer>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<Features.Profiles.ProfileService>();
        services.AddScoped<Features.Resume.EducationService>();
        services.AddScoped<Features.Resume.WorkExperienceService>();
        services.AddScoped<Features.Skills.SkillService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<ICalendarEventService, CalendarEventService>();
        services.AddScoped<ITodoItemService, TodoItemService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IWorkTaskService, WorkTaskService>();
        services.AddScoped<Features.Blog.BlogService>();
        services.AddScoped<Features.Blog.TagService>();
        services.AddScoped<IGuestBookEntryService, GuestBookEntryService>();
        services.AddScoped<Features.Contacts.ContactMethodService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<IPortfolioAttachmentService, PortfolioAttachmentService>();
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        return services;
    }
}
