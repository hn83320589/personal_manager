using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Features.Auth;

public static class AccountRules
{
    /// <summary>帳號會出現在公開網址 /@username，只允許英數、底線與連字號。</summary>
    public const string UsernamePattern = "^[A-Za-z0-9_-]{3,30}$";

    public const int MinPasswordLength = 8;
}

public sealed record LoginRequest(
    [Required(ErrorMessage = "請輸入帳號")] string Username,
    [Required(ErrorMessage = "請輸入密碼")] string Password);

public sealed record RegisterRequest(
    [Required, RegularExpression(AccountRules.UsernamePattern, ErrorMessage = "帳號需為 3–30 個英文字母、數字、底線或連字號")] string Username,
    [Required, EmailAddress(ErrorMessage = "Email 格式不正確"), StringLength(200)] string Email,
    [Required, StringLength(100, MinimumLength = AccountRules.MinPasswordLength, ErrorMessage = "密碼至少 8 個字元")] string Password,
    [Required(ErrorMessage = "請輸入姓名"), StringLength(100)] string FullName);

public sealed record ForgotPasswordRequest([Required, EmailAddress] string Email);

public sealed record ResetPasswordRequest(
    [Required] string Token,
    [Required, StringLength(100, MinimumLength = AccountRules.MinPasswordLength, ErrorMessage = "密碼至少 8 個字元")] string NewPassword);

public sealed record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, StringLength(100, MinimumLength = AccountRules.MinPasswordLength, ErrorMessage = "密碼至少 8 個字元")] string NewPassword);

public sealed record AuthUserDto(int Id, string Username, string Email, string FullName, string Role);

/// <summary>回應本文只含 access token；refresh token 只以 httpOnly cookie 傳遞。</summary>
public sealed record AccessTokenDto(string AccessToken, DateTime ExpiresAt, AuthUserDto User);

/// <summary>service 回給 controller 的完整結果，controller 負責把 refresh token 放進 cookie。</summary>
public sealed record AuthSession(AccessTokenDto Access, string RefreshToken, DateTime RefreshTokenExpiresAt);
