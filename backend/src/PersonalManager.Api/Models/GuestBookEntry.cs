using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public class GuestBookEntry
{
    public int Id { get; set; }
    /// <summary>留言板的主人（被留言的使用者）。沒有預設值：未指定時不能默默送到某位使用者。</summary>
    public int TargetUserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public bool IsApproved { get; set; }

    public string AdminReply { get; set; } = string.Empty;
    public DateTime? RepliedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
