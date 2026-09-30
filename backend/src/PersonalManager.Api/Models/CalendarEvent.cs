using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class CalendarEvent : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsAllDay { get; set; }
    public bool IsPublic { get; set; }

    [StringLength(20)]
    public string Color { get; set; } = string.Empty;

    public Recurrence Recurrence { get; set; } = Recurrence.None;

    /// <summary>重複到這一天為止（含）；null 表示一直重複。</summary>
    public DateOnly? RecurrenceUntil { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>重複規則：每期從原始開始時間往後推算，不會因月底日數不同而累積偏移。</summary>
public enum Recurrence { None, Daily, Weekly, Monthly, Yearly }
