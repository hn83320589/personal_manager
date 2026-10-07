using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public enum ContactType
{
    Email,
    Phone,
    LinkedIn,
    GitHub,
    Facebook,
    Twitter,
    Instagram,
    Discord,
    Other,
    Behance,
    Dribbble,
    YouTube,
    Threads,
    Line,
    Website
}

public class ContactMethod : IOwnedByUser, ISortable
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public ContactType Type { get; set; }

    [StringLength(50)]
    public string Label { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;

    [StringLength(50)]
    public string Icon { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
