using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class WorkExperience : IOwnedByUser, ISortable
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Company { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Position { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
