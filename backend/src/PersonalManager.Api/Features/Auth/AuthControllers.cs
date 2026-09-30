using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Setup;

namespace PersonalManager.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>refresh token 只放在這個 cookie：JavaScript 讀不到、只走 https、只送往 /api/auth（ADR-010）。</summary>
    public const string RefreshCookieName = "pm_refresh";
    private const string RefreshCookiePath = "/api/auth";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ApiResponse<AccessTokenDto>> Login(LoginRequest request) =>
        ApiResponse<AccessTokenDto>.Ok(IssueCookie(await auth.LoginAsync(request)), "登入成功");

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AccessTokenDto>>> Register(RegisterRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<AccessTokenDto>.Ok(IssueCookie(await auth.RegisterAsync(request)), "註冊成功"));

    [AllowAnonymous]
    [HttpPost("refresh")]
    [EnableRateLimiting(RateLimitPolicies.Session)]
    public async Task<ApiResponse<AccessTokenDto>> Refresh() =>
        ApiResponse<AccessTokenDto>.Ok(IssueCookie(await auth.RefreshAsync(Request.Cookies[RefreshCookieName])));

    [AllowAnonymous]
    [HttpPost("logout")]
    [EnableRateLimiting(RateLimitPolicies.Session)]
    public async Task<ApiResponse> Logout()
    {
        await auth.LogoutAsync(Request.Cookies[RefreshCookieName]);
        Response.Cookies.Delete(RefreshCookieName, CookieOptions(expires: null));
        return ApiResponse.Ok("已登出");
    }

    [Authorize]
    [HttpGet("me")]
    [DisableRateLimiting]
    public async Task<ApiResponse<AuthUserDto>> Me() => ApiResponse<AuthUserDto>.Ok(await auth.GetCurrentUserAsync());

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ApiResponse> ForgotPassword(ForgotPasswordRequest request)
    {
        await auth.ForgotPasswordAsync(request);
        return ApiResponse.Ok("如果這個 Email 有註冊，我們已寄出重設密碼的連結");
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ApiResponse> ResetPassword(ResetPasswordRequest request)
    {
        await auth.ResetPasswordAsync(request);
        return ApiResponse.Ok("密碼已重設，請使用新密碼登入");
    }

    private AccessTokenDto IssueCookie(AuthSession session)
    {
        Response.Cookies.Append(RefreshCookieName, session.RefreshToken, CookieOptions(session.RefreshTokenExpiresAt));
        return session.Access;
    }

    private static CookieOptions CookieOptions(DateTime? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = RefreshCookiePath,
        Expires = expires
    };
}

[ApiController]
[Authorize]
[Route("api/me/password")]
public sealed class MyPasswordController(AuthService auth) : ControllerBase
{
    [HttpPut]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<ApiResponse> Change(ChangePasswordRequest request)
    {
        await auth.ChangePasswordAsync(request);
        return ApiResponse.Ok("已更新密碼，其他裝置需要重新登入");
    }
}
