using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonalManager.Api.Auth;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;
using PersonalManager.Api.Services;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Features.Auth;

public sealed class AuthService(
    ApplicationDbContext db,
    ICurrentUser currentUser,
    IOptions<JwtSettings> jwtOptions,
    IOptions<EmailSettings> emailOptions,
    IEmailService email,
    TimeProvider clock)
{
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);
    private const string InvalidCredentials = "帳號或密碼錯誤";
    private const string SessionExpired = "登入已失效，請重新登入";

    private JwtSettings Jwt => jwtOptions.Value;
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AuthSession> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        // 帳號不存在、密碼錯誤、帳號停用都回同一個訊息，不透露帳號是否存在
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthenticatedException(InvalidCredentials);
        return await StartSessionAsync(user);
    }

    public async Task<AuthSession> RegisterAsync(RegisterRequest request)
    {
        var username = request.Username.Trim();
        var emailAddress = request.Email.Trim();
        var taken = await db.Users.AnyAsync(u =>
            u.Username.ToLower() == username.ToLower() || u.Email.ToLower() == emailAddress.ToLower());
        if (taken)
            throw new ConflictException("這個帳號或 Email 已經被使用");

        var user = new User
        {
            Username = username,
            Email = emailAddress,
            FullName = request.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return await StartSessionAsync(user);
    }

    /// <summary>
    /// 以 refresh token 換發新的一組 token，舊的立即撤銷。
    /// 已撤銷的 token 再次出現，表示它可能被竊取後由另一方使用：撤銷該使用者全部的工作階段。
    /// </summary>
    public async Task<AuthSession> RefreshAsync(string? rawToken)
    {
        if (string.IsNullOrEmpty(rawToken))
            throw new UnauthenticatedException(SessionExpired);

        var hash = OpaqueTokens.Hash(rawToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash)
                     ?? throw new UnauthenticatedException(SessionExpired);

        if (stored.IsRevoked)
        {
            await RevokeAllSessionsAsync(stored.UserId);
            throw new UnauthenticatedException(SessionExpired);
        }

        var user = await db.Users.FirstAsync(u => u.Id == stored.UserId);
        if (stored.ExpiresAt <= Now || !user.IsActive)
            throw new UnauthenticatedException(SessionExpired);

        stored.IsRevoked = true;
        return await StartSessionAsync(user);
    }

    public async Task LogoutAsync(string? rawToken)
    {
        if (string.IsNullOrEmpty(rawToken))
            return;
        var hash = OpaqueTokens.Hash(rawToken);
        await db.RefreshTokens.Where(t => t.TokenHash == hash)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true));
    }

    public async Task<AuthUserDto> GetCurrentUserAsync()
    {
        var userId = currentUser.RequireUserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive)
                   ?? throw new UnauthenticatedException(SessionExpired);
        return ToDto(user);
    }

    /// <summary>不論 Email 是否存在都回應成功，避免被用來查詢某個 Email 是否註冊過。</summary>
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower() && u.IsActive);
        if (user is null)
            return;

        var token = OpaqueTokens.Create();
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = OpaqueTokens.Hash(token),
            ExpiresAt = Now + ResetTokenLifetime
        });
        await db.SaveChangesAsync();

        var link = $"{emailOptions.Value.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={token}";
        await email.SendEmailAsync(user.Email, "重設你的 Personal Manager 密碼", $"""
            <p>{WebUtility.HtmlEncode(user.FullName)} 你好：</p>
            <p>我們收到重設密碼的要求，請在一小時內點擊下方連結設定新密碼：</p>
            <p><a href="{link}">{link}</a></p>
            <p>如果這不是你本人的操作，請忽略這封信，你的密碼不會改變。</p>
            """);
    }

    /// <summary>重設後撤銷所有登入中的工作階段與其他尚未使用的重設連結。</summary>
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var hash = OpaqueTokens.Hash(request.Token);
        var reset = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (reset is null || reset.IsUsed || reset.ExpiresAt <= Now)
            throw new DomainValidationException("重設連結無效或已過期，請重新申請");

        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await db.Users.FirstAsync(u => u.Id == reset.UserId);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = Now;
        await db.SaveChangesAsync();
        await db.PasswordResetTokens.Where(t => t.UserId == user.Id && !t.IsUsed)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsUsed, true));
        await RevokeAllSessionsAsync(user.Id);
        await transaction.CommitAsync();
    }

    /// <summary>修改密碼後撤銷所有工作階段，其他裝置需重新登入；目前裝置的 access token 仍可用到過期。</summary>
    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var userId = currentUser.RequireUserId();
        var user = await db.Users.FirstAsync(u => u.Id == userId);
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainValidationException("目前的密碼不正確");

        await using var transaction = await db.Database.BeginTransactionAsync();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = Now;
        await db.SaveChangesAsync();
        await RevokeAllSessionsAsync(userId);
        await transaction.CommitAsync();
    }

    private Task RevokeAllSessionsAsync(int userId) =>
        db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true));

    private async Task<AuthSession> StartSessionAsync(User user)
    {
        var refreshToken = OpaqueTokens.Create();
        var refreshExpiresAt = Now.AddDays(Jwt.RefreshTokenDays);
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = OpaqueTokens.Hash(refreshToken),
            ExpiresAt = refreshExpiresAt
        });
        await db.SaveChangesAsync();

        var accessExpiresAt = Now.AddMinutes(Jwt.AccessTokenMinutes);
        return new AuthSession(new AccessTokenDto(CreateAccessToken(user, accessExpiresAt), accessExpiresAt, ToDto(user)),
            refreshToken, refreshExpiresAt);
    }

    private string CreateAccessToken(User user, DateTime expiresAt)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        ];
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.SecretKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(Jwt.Issuer, Jwt.Audience, claims,
            notBefore: Now, expires: expiresAt, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthUserDto ToDto(User u) => new(u.Id, u.Username, u.Email, u.FullName, u.Role);
}
