using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Mappings;

public static class MappingExtensions
{
    // ===== User =====
    public static UserResponse ToResponse(this User u) => new()
    {
        Id = u.Id, Username = u.Username, Email = u.Email,
        FullName = u.FullName, Role = u.Role, IsActive = u.IsActive, CreatedAt = u.CreatedAt
    };
    public static User ToEntity(this CreateUserDto d) => new()
    {
        Username = d.Username, Email = d.Email, FullName = d.FullName, Role = d.Role
    };
    public static void ApplyUpdate(this User u, UpdateUserDto d)
    {
        if (d.Email != null) u.Email = d.Email;
        if (d.FullName != null) u.FullName = d.FullName;
        if (d.Role != null) u.Role = d.Role;
        if (d.IsActive.HasValue) u.IsActive = d.IsActive.Value;
    }

    // ===== Portfolio =====
    public static PortfolioResponse ToResponse(this Portfolio p) => new()
    {
        Id = p.Id, UserId = p.UserId, Title = p.Title, Description = p.Description,
        ImageUrl = p.ImageUrl, ProjectUrl = p.ProjectUrl, RepositoryUrl = p.RepositoryUrl,
        Technologies = p.Technologies, IsFeatured = p.IsFeatured, IsPublic = p.IsPublic,
        SortOrder = p.SortOrder, CreatedAt = p.CreatedAt
    };
    public static Portfolio ToEntity(this CreatePortfolioDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        ImageUrl = d.ImageUrl, ProjectUrl = d.ProjectUrl, RepositoryUrl = d.RepositoryUrl,
        Technologies = d.Technologies, IsFeatured = d.IsFeatured,
        IsPublic = d.IsPublic, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this Portfolio p, UpdatePortfolioDto d)
    {
        if (d.Title != null) p.Title = d.Title;
        if (d.Description != null) p.Description = d.Description;
        if (d.ImageUrl != null) p.ImageUrl = d.ImageUrl;
        if (d.ProjectUrl != null) p.ProjectUrl = d.ProjectUrl;
        if (d.RepositoryUrl != null) p.RepositoryUrl = d.RepositoryUrl;
        if (d.Technologies != null) p.Technologies = d.Technologies;
        if (d.IsFeatured.HasValue) p.IsFeatured = d.IsFeatured.Value;
        if (d.IsPublic.HasValue) p.IsPublic = d.IsPublic.Value;
        if (d.SortOrder.HasValue) p.SortOrder = d.SortOrder.Value;
    }

    // ===== TodoItem =====
    public static TodoItemResponse ToResponse(this TodoItem t) => new()
    {
        Id = t.Id, UserId = t.UserId, Title = t.Title, Description = t.Description,
        Priority = t.Priority, Status = t.Status, DueDate = t.DueDate,
        CompletedAt = t.CompletedAt, CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
    };
    public static TodoItem ToEntity(this CreateTodoItemDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        Priority = d.Priority, Status = d.Status, DueDate = d.DueDate
    };
    public static void ApplyUpdate(this TodoItem t, UpdateTodoItemDto d)
    {
        if (d.Title != null) t.Title = d.Title;
        if (d.Description != null) t.Description = d.Description;
        if (d.Priority.HasValue) t.Priority = d.Priority.Value;
        if (d.Status.HasValue) t.Status = d.Status.Value;
        if (d.DueDate.HasValue) t.DueDate = d.DueDate;
        if (d.CompletedAt.HasValue) t.CompletedAt = d.CompletedAt;
    }

    // ===== WorkTask =====
    public static WorkTaskResponse ToResponse(this WorkTask w, string? projectName = null) => new()
    {
        Id = w.Id, UserId = w.UserId, Title = w.Title, Description = w.Description,
        ProjectId = w.ProjectId, ProjectName = projectName,
        Priority = w.Priority, Status = w.Status,
        EstimatedHours = w.EstimatedHours, ActualHours = w.ActualHours,
        DueDate = w.DueDate, CompletedAt = w.CompletedAt, Tags = w.Tags,
        CreatedAt = w.CreatedAt, UpdatedAt = w.UpdatedAt
    };
    public static WorkTask ToEntity(this CreateWorkTaskDto d) => new()
    {
        UserId = d.UserId, Title = d.Title, Description = d.Description,
        ProjectId = d.ProjectId, Priority = d.Priority, Status = d.Status,
        EstimatedHours = d.EstimatedHours, DueDate = d.DueDate, Tags = d.Tags
    };
    public static void ApplyUpdate(this WorkTask w, UpdateWorkTaskDto d)
    {
        if (d.Title != null) w.Title = d.Title;
        if (d.Description != null) w.Description = d.Description;
        if (d.ProjectId.HasValue) w.ProjectId = d.ProjectId;
        if (d.Priority.HasValue) w.Priority = d.Priority.Value;
        if (d.Status.HasValue) w.Status = d.Status.Value;
        if (d.EstimatedHours.HasValue) w.EstimatedHours = d.EstimatedHours.Value;
        if (d.ActualHours.HasValue) w.ActualHours = d.ActualHours.Value;
        if (d.DueDate.HasValue) w.DueDate = d.DueDate;
        if (d.CompletedAt.HasValue) w.CompletedAt = d.CompletedAt;
        if (d.Tags != null) w.Tags = d.Tags;
    }

    // ===== Project =====
    public static ProjectResponse ToResponse(this Project p) => new()
    {
        Id = p.Id, UserId = p.UserId, Name = p.Name, Description = p.Description,
        Color = p.Color, SortOrder = p.SortOrder, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
    public static Project ToEntity(this CreateProjectDto d) => new()
    {
        UserId = d.UserId, Name = d.Name, Description = d.Description,
        Color = d.Color, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this Project p, UpdateProjectDto d)
    {
        if (d.Name != null) p.Name = d.Name;
        if (d.Description != null) p.Description = d.Description;
        if (d.Color != null) p.Color = d.Color;
        if (d.SortOrder.HasValue) p.SortOrder = d.SortOrder.Value;
    }

    // ===== FileUpload =====
    public static FileUploadResponse ToResponse(this FileUpload f) => new()
    {
        Id = f.Id, UserId = f.UserId, FileName = f.FileName, StoredName = f.StoredName,
        FileUrl = f.FileUrl, FileType = f.FileType, FileSize = f.FileSize,
        MimeType = f.MimeType, CreatedAt = f.CreatedAt
    };

    // ===== PortfolioAttachment =====
    public static PortfolioAttachmentResponse ToResponse(this PortfolioAttachment a) => new()
    {
        Id = a.Id, PortfolioId = a.PortfolioId, FileUploadId = a.FileUploadId,
        FileName = a.FileName, FileUrl = a.FileUrl, FileType = a.FileType,
        FileSize = a.FileSize, SortOrder = a.SortOrder, CreatedAt = a.CreatedAt
    };
    public static PortfolioAttachment ToEntity(this CreatePortfolioAttachmentDto d) => new()
    {
        PortfolioId = d.PortfolioId, FileUploadId = d.FileUploadId, FileName = d.FileName,
        FileUrl = d.FileUrl, FileType = d.FileType, FileSize = d.FileSize, SortOrder = d.SortOrder
    };
    public static void ApplyUpdate(this PortfolioAttachment a, UpdatePortfolioAttachmentDto d)
    {
        if (d.SortOrder.HasValue) a.SortOrder = d.SortOrder.Value;
    }

    // ===== TimeEntry =====
    public static TimeEntryResponse ToResponse(this TimeEntry t) => new()
    {
        Id = t.Id, UserId = t.UserId, WorkTaskId = t.WorkTaskId,
        Task = t.Task, Project = t.Project, Date = t.Date,
        StartTime = t.StartTime, EndTime = t.EndTime,
        Duration = t.Duration, Description = t.Description,
        CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
    };
    public static TimeEntry ToEntity(this CreateTimeEntryDto d) => new()
    {
        UserId = d.UserId, WorkTaskId = d.WorkTaskId,
        Task = d.Task, Project = d.Project, Date = d.Date,
        StartTime = d.StartTime, EndTime = d.EndTime,
        Duration = d.Duration, Description = d.Description
    };
    public static void ApplyUpdate(this TimeEntry t, UpdateTimeEntryDto d)
    {
        if (d.WorkTaskId.HasValue) t.WorkTaskId = d.WorkTaskId;
        if (d.Task != null) t.Task = d.Task;
        if (d.Project != null) t.Project = d.Project;
        if (d.Date != null) t.Date = d.Date;
        if (d.StartTime != null) t.StartTime = d.StartTime;
        if (d.EndTime != null) t.EndTime = d.EndTime;
        if (d.Duration.HasValue) t.Duration = d.Duration.Value;
        if (d.Description != null) t.Description = d.Description;
        t.UpdatedAt = DateTime.UtcNow;
    }
}
