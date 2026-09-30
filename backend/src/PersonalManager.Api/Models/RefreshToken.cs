using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

/// <summary>
/// 登入工作階段。實際 token 只存在使用者瀏覽器的 httpOnly cookie，資料庫只保存 SHA-256 雜湊（ADR-010）。
/// 每次 refresh 都會撤銷舊的並發新的；已撤銷的 token 再被使用代表可能外洩，會撤銷該使用者全部的工作階段。
/// </summary>
public class RefreshToken : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
