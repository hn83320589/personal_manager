using System.Security.Cryptography;
using System.Text;

namespace PersonalManager.Api.Features.Auth;

/// <summary>refresh token 與重設密碼 token：隨機產生，資料庫只保存雜湊。</summary>
public static class OpaqueTokens
{
    /// <summary>384 bits 的隨機值，以 URL 安全的 Base64 表示（可直接放進 cookie 與網址）。</summary>
    public static string Create() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    /// <summary>SHA-256 十六進位字串。token 本身已是高熵隨機值，不需要加鹽或慢速雜湊。</summary>
    public static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
