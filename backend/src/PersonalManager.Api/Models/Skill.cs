using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalManager.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SkillLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

public class Skill : IOwnedByUser, ISortable
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>選填：不是每種職業都習慣標示熟練度。</summary>
    public SkillLevel? Level { get; set; }

    /// <summary>選填。</summary>
    public int? YearsOfExperience { get; set; }

    public bool IsPublic { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
