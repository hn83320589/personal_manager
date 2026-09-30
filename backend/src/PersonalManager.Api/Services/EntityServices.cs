using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Mappings;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Services;

// ===== User Service =====
public interface IUserService : ICrudService<User, CreateUserDto, UpdateUserDto, UserResponse> { }

public class UserService : CrudService<User, CreateUserDto, UpdateUserDto, UserResponse>, IUserService
{
    public UserService(IRepository<User> repo) : base(repo) { }
    protected override User MapToEntity(CreateUserDto dto)
    {
        var entity = dto.ToEntity();
        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        return entity;
    }
    protected override UserResponse MapToResponse(User entity) => entity.ToResponse();
    protected override void ApplyUpdate(User entity, UpdateUserDto dto) => entity.ApplyUpdate(dto);
}

// ===== Portfolio Service =====
public interface IPortfolioService : ICrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>
{
    Task<List<PortfolioResponse>> GetByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetFeaturedAsync(int userId);
}

public class PortfolioService : CrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>, IPortfolioService
{
    public PortfolioService(IRepository<Portfolio> repo) : base(repo) { }
    protected override Portfolio MapToEntity(CreatePortfolioDto dto) => dto.ToEntity();
    protected override PortfolioResponse MapToResponse(Portfolio entity) => entity.ToResponse();
    protected override void ApplyUpdate(Portfolio entity, UpdatePortfolioDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetFeaturedAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsFeatured && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== CalendarEvent Service =====
public interface ICalendarEventService : ICrudService<CalendarEvent, CreateCalendarEventDto, UpdateCalendarEventDto, CalendarEventResponse>
{
    Task<List<CalendarEventResponse>> GetByUserIdAsync(int userId);
    Task<List<CalendarEventResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<CalendarEventResponse>> GetByDateRangeAsync(int userId, DateTime start, DateTime end);
}

public class CalendarEventService : CrudService<CalendarEvent, CreateCalendarEventDto, UpdateCalendarEventDto, CalendarEventResponse>, ICalendarEventService
{
    public CalendarEventService(IRepository<CalendarEvent> repo) : base(repo) { }
    protected override CalendarEvent MapToEntity(CreateCalendarEventDto dto) => dto.ToEntity();
    protected override CalendarEventResponse MapToResponse(CalendarEvent entity) => entity.ToResponse();
    protected override void ApplyUpdate(CalendarEvent entity, UpdateCalendarEventDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<CalendarEventResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId);
        return items.OrderBy(c => c.StartTime).Select(MapToResponse).ToList();
    }

    public async Task<List<CalendarEventResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId && c.IsPublic);
        return items.OrderBy(c => c.StartTime).Select(MapToResponse).ToList();
    }

    public async Task<List<CalendarEventResponse>> GetByDateRangeAsync(int userId, DateTime start, DateTime end)
    {
        var items = await Repository.FindAsync(c => c.UserId == userId && c.StartTime >= start && c.EndTime <= end);
        return items.OrderBy(c => c.StartTime).Select(MapToResponse).ToList();
    }
}

// ===== TodoItem Service =====
public interface ITodoItemService : ICrudService<TodoItem, CreateTodoItemDto, UpdateTodoItemDto, TodoItemResponse>
{
    Task<List<TodoItemResponse>> GetByUserIdAsync(int userId);
    Task<List<TodoItemResponse>> GetByStatusAsync(int userId, TodoStatus status);
}

public class TodoItemService : CrudService<TodoItem, CreateTodoItemDto, UpdateTodoItemDto, TodoItemResponse>, ITodoItemService
{
    public TodoItemService(IRepository<TodoItem> repo) : base(repo) { }
    protected override TodoItem MapToEntity(CreateTodoItemDto dto) => dto.ToEntity();
    protected override TodoItemResponse MapToResponse(TodoItem entity) => entity.ToResponse();
    protected override void ApplyUpdate(TodoItem entity, UpdateTodoItemDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<TodoItemResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(t => t.UserId == userId);
        return items.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).Select(MapToResponse).ToList();
    }

    public async Task<List<TodoItemResponse>> GetByStatusAsync(int userId, TodoStatus status)
    {
        var items = await Repository.FindAsync(t => t.UserId == userId && t.Status == status);
        return items.OrderByDescending(t => t.Priority).Select(MapToResponse).ToList();
    }
}

// ===== Project Service =====
public interface IProjectService : ICrudService<Project, CreateProjectDto, UpdateProjectDto, ProjectResponse>
{
    Task<List<ProjectResponse>> GetByUserIdAsync(int userId);
}

public class ProjectService : CrudService<Project, CreateProjectDto, UpdateProjectDto, ProjectResponse>, IProjectService
{
    public ProjectService(IRepository<Project> repo) : base(repo) { }
    protected override Project MapToEntity(CreateProjectDto dto) => dto.ToEntity();
    protected override ProjectResponse MapToResponse(Project entity) => entity.ToResponse();
    protected override void ApplyUpdate(Project entity, UpdateProjectDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<ProjectResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId);
        return items.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).Select(MapToResponse).ToList();
    }
}

// ===== WorkTask Service =====
public interface IWorkTaskService : ICrudService<WorkTask, CreateWorkTaskDto, UpdateWorkTaskDto, WorkTaskResponse>
{
    Task<List<WorkTaskResponse>> GetByUserIdAsync(int userId);
    Task<List<WorkTaskResponse>> GetByProjectIdAsync(int userId, int? projectId);
    Task<List<WorkTaskResponse>> GetByStatusAsync(int userId, WorkTaskStatus status);
}

public class WorkTaskService : CrudService<WorkTask, CreateWorkTaskDto, UpdateWorkTaskDto, WorkTaskResponse>, IWorkTaskService
{
    private readonly IRepository<Project> _projectRepo;

    public WorkTaskService(IRepository<WorkTask> repo, IRepository<Project> projectRepo) : base(repo)
    {
        _projectRepo = projectRepo;
    }

    protected override WorkTask MapToEntity(CreateWorkTaskDto dto) => dto.ToEntity();
    protected override WorkTaskResponse MapToResponse(WorkTask entity) => entity.ToResponse();
    protected override void ApplyUpdate(WorkTask entity, UpdateWorkTaskDto dto) => entity.ApplyUpdate(dto);

    private async Task<Dictionary<int, string>> GetProjectMapAsync(IEnumerable<WorkTask> tasks)
    {
        var projectIds = tasks.Where(t => t.ProjectId.HasValue).Select(t => t.ProjectId!.Value).Distinct().ToList();
        if (projectIds.Count == 0) return new();
        var projects = await _projectRepo.FindAsync(p => projectIds.Contains(p.Id));
        return projects.ToDictionary(p => p.Id, p => p.Name);
    }

    public async Task<List<WorkTaskResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId);
        var projectMap = await GetProjectMapAsync(items);
        return items.OrderByDescending(w => w.Priority).ThenBy(w => w.DueDate)
            .Select(w => w.ToResponse(w.ProjectId.HasValue ? projectMap.GetValueOrDefault(w.ProjectId.Value) : null)).ToList();
    }

    public async Task<List<WorkTaskResponse>> GetByProjectIdAsync(int userId, int? projectId)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId && w.ProjectId == projectId);
        var projectMap = await GetProjectMapAsync(items);
        return items.OrderByDescending(w => w.Priority)
            .Select(w => w.ToResponse(w.ProjectId.HasValue ? projectMap.GetValueOrDefault(w.ProjectId.Value) : null)).ToList();
    }

    public async Task<List<WorkTaskResponse>> GetByStatusAsync(int userId, WorkTaskStatus status)
    {
        var items = await Repository.FindAsync(w => w.UserId == userId && w.Status == status);
        var projectMap = await GetProjectMapAsync(items);
        return items.OrderByDescending(w => w.Priority)
            .Select(w => w.ToResponse(w.ProjectId.HasValue ? projectMap.GetValueOrDefault(w.ProjectId.Value) : null)).ToList();
    }
}

// ===== FileUpload Service =====
public interface IFileUploadService
{
    Task<List<FileUploadResponse>> GetByUserIdAsync(int userId);
    Task<FileUploadResponse> UploadAsync(IFormFile file, int userId);
    Task<bool> DeleteAsync(int id, int userId);
}

public class FileUploadService : IFileUploadService
{
    private readonly IRepository<FileUpload> _repo;
    private readonly IFileStorageProvider _storage;

    public FileUploadService(IRepository<FileUpload> repo, IFileStorageProvider storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<List<FileUploadResponse>> GetByUserIdAsync(int userId)
    {
        var items = await _repo.FindAsync(f => f.UserId == userId);
        return items.OrderByDescending(f => f.CreatedAt).Select(f => f.ToResponse()).ToList();
    }

    public async Task<FileUploadResponse> UploadAsync(IFormFile file, int userId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var (fileUrl, storedName) = await _storage.UploadAsync(file, ext);

        var fileType = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
            ".pdf" => "pdf",
            ".doc" or ".docx" => "document",
            ".ppt" or ".pptx" => "presentation",
            _ => "other"
        };

        var entity = new FileUpload
        {
            UserId = userId,
            FileName = file.FileName,
            StoredName = storedName,
            FileUrl = fileUrl,
            FileType = fileType,
            FileSize = file.Length,
            MimeType = file.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repo.AddAsync(entity);
        return saved.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var items = await _repo.FindAsync(f => f.Id == id && f.UserId == userId);
        var entity = items.FirstOrDefault();
        if (entity == null) return false;

        await _storage.DeleteAsync(entity.StoredName);
        return await _repo.DeleteAsync(id);
    }
}

// ===== PortfolioAttachment Service =====
public interface IPortfolioAttachmentService : ICrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>
{
    Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId);
}

public class PortfolioAttachmentService : CrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>, IPortfolioAttachmentService
{
    public PortfolioAttachmentService(IRepository<PortfolioAttachment> repo) : base(repo) { }
    protected override PortfolioAttachment MapToEntity(CreatePortfolioAttachmentDto dto) => dto.ToEntity();
    protected override PortfolioAttachmentResponse MapToResponse(PortfolioAttachment entity) => entity.ToResponse();
    protected override void ApplyUpdate(PortfolioAttachment entity, UpdatePortfolioAttachmentDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId)
    {
        var items = await Repository.FindAsync(a => a.PortfolioId == portfolioId);
        return items.OrderBy(a => a.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== GuestBookEntry Service =====
public interface IGuestBookEntryService : ICrudService<GuestBookEntry, CreateGuestBookEntryDto, UpdateGuestBookEntryDto, GuestBookEntryResponse>
{
    Task<List<GuestBookEntryResponse>> GetApprovedAsync();
    Task<List<GuestBookEntryResponse>> GetApprovedByTargetUserIdAsync(int targetUserId);
    Task<PagedResult<GuestBookEntryResponse>> GetApprovedPagedAsync(int targetUserId, int page, int pageSize);
}

public class GuestBookEntryService : CrudService<GuestBookEntry, CreateGuestBookEntryDto, UpdateGuestBookEntryDto, GuestBookEntryResponse>, IGuestBookEntryService
{
    public GuestBookEntryService(IRepository<GuestBookEntry> repo) : base(repo) { }
    protected override GuestBookEntry MapToEntity(CreateGuestBookEntryDto dto) => dto.ToEntity();
    protected override GuestBookEntryResponse MapToResponse(GuestBookEntry entity) => entity.ToResponse();
    protected override void ApplyUpdate(GuestBookEntry entity, UpdateGuestBookEntryDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<GuestBookEntryResponse>> GetApprovedAsync()
    {
        var items = await Repository.FindAsync(g => g.IsApproved);
        return items.OrderByDescending(g => g.CreatedAt).Select(MapToResponse).ToList();
    }

    public async Task<List<GuestBookEntryResponse>> GetApprovedByTargetUserIdAsync(int targetUserId)
    {
        var items = await Repository.FindAsync(g => g.TargetUserId == targetUserId && g.IsApproved);
        return items.OrderByDescending(g => g.CreatedAt).Select(MapToResponse).ToList();
    }

    public async Task<PagedResult<GuestBookEntryResponse>> GetApprovedPagedAsync(int targetUserId, int page, int pageSize)
    {
        var items = await Repository.FindAsync(g => g.TargetUserId == targetUserId && g.IsApproved);
        var ordered = items.OrderByDescending(g => g.CreatedAt).ToList();
        return new PagedResult<GuestBookEntryResponse>
        {
            Items = ordered.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList(),
            TotalCount = ordered.Count,
            Page = page,
            PageSize = pageSize
        };
    }
}

// ===== TimeEntry Service =====
public interface ITimeEntryService : ICrudService<TimeEntry, CreateTimeEntryDto, UpdateTimeEntryDto, TimeEntryResponse>
{
    Task<List<TimeEntryResponse>> GetByUserIdAsync(int userId);
}

public class TimeEntryService : CrudService<TimeEntry, CreateTimeEntryDto, UpdateTimeEntryDto, TimeEntryResponse>, ITimeEntryService
{
    public TimeEntryService(IRepository<TimeEntry> repo) : base(repo) { }
    protected override TimeEntry MapToEntity(CreateTimeEntryDto dto) => dto.ToEntity();
    protected override TimeEntryResponse MapToResponse(TimeEntry entity) => entity.ToResponse();
    protected override void ApplyUpdate(TimeEntry entity, UpdateTimeEntryDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<TimeEntryResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(t => t.UserId == userId);
        return items.OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt).Select(MapToResponse).ToList();
    }
}
