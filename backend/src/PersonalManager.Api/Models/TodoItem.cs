using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public enum TodoPriority
{
    Low,
    Medium,
    High
}

public enum TodoStatus
{
    Pending,
    InProgress,
    Completed
}

public class TodoItem : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
