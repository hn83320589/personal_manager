using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

/// <summary>重設密碼連結。資料庫只保存 SHA-256 雜湊，一小時內有效且只能使用一次。</summary>
public class PasswordResetToken : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
