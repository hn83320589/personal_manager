using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Features.Guestbook;

/// <summary>訪客留言。Email 選填，只有版主看得到。</summary>
public sealed record LeaveMessageRequest(
    [Required(ErrorMessage = "請輸入名字"), StringLength(50)] string Name,
    [EmailAddress(ErrorMessage = "Email 格式不正確"), StringLength(200)] string? Email,
    [Required(ErrorMessage = "請輸入留言內容"), StringLength(2000)] string Message);

public sealed record SetApprovalRequest(bool IsApproved);

public sealed record ReplyRequest([StringLength(2000)] string? Reply);

/// <summary>公開顯示：不含 Email。</summary>
public sealed record PublicGuestbookEntryDto(int Id, string Name, string Message, string Reply, DateTime CreatedAt, DateTime? RepliedAt);

public sealed record GuestbookEntryDto(
    int Id, string Name, string Email, string Message, bool IsApproved, string Reply, DateTime CreatedAt, DateTime? RepliedAt);

public enum GuestbookFilter { All, Pending, Approved }
