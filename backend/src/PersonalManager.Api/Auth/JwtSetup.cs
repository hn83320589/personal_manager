using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PersonalManager.Api.Auth;

public static class JwtSetup
{
    /// <summary>HMAC-SHA256 要求金鑰至少 256 bits。</summary>
    public const int MinimumSecretLength = 32;

    private const string PlaceholderPrefix = "CHANGE_THIS";

    /// <summary>
    /// 讀取並驗證 JWT 設定。金鑰未設定、仍是占位字串或長度不足時：
    /// Development 產生一把臨時隨機金鑰（重新啟動後舊 token 失效）；其他環境直接中止啟動，
    /// 避免以公開已知的金鑰簽發可被偽造的 token。
    /// </summary>
    public static JwtSettings LoadJwtSettings(this IConfiguration configuration, IHostEnvironment environment, ILogger logger)
    {
        var settings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        if (IsUsable(settings.SecretKey))
            return settings;

        if (!environment.IsDevelopment())
            throw new InvalidOperationException(
                $"Jwt:SecretKey 未設定、仍是占位字串，或短於 {MinimumSecretLength} 字元。" +
                "請以環境變數 Jwt__SecretKey 提供一組隨機產生的金鑰。");

        settings.SecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        logger.LogWarning(
            "Jwt:SecretKey 未設定，已產生臨時金鑰；API 重新啟動後需要重新登入。" +
            "可在 appsettings.Development.json 設定固定的金鑰。");
        return settings;
    }

    /// <summary>
    /// 註冊 JWT 驗證。驗證後的設定同時供 JwtBearer 驗簽與 AuthService 簽發使用，確保兩邊是同一把金鑰。
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment, ILogger logger)
    {
        var settings = configuration.LoadJwtSettings(environment, logger);
        services.AddSingleton(Options.Create(settings));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = settings.Issuer,
                ValidAudience = settings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey))
            });
        services.AddAuthorization();
        return services;
    }

    private static bool IsUsable(string? secret) =>
        !string.IsNullOrWhiteSpace(secret)
        && secret.Length >= MinimumSecretLength
        && !secret.StartsWith(PlaceholderPrefix, StringComparison.Ordinal);
}
