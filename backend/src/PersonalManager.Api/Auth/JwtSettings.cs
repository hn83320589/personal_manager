namespace PersonalManager.Api.Auth;

public class JwtSettings
{
    /// <summary>
    /// HMAC-SHA256 簽章金鑰。刻意沒有預設值：來源只能是設定檔或環境變數（<c>Jwt__SecretKey</c>），
    /// 由 <see cref="JwtSetup.LoadJwtSettings"/> 在啟動時驗證。
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PersonalManagerAPI";
    public string Audience { get; set; } = "PersonalManagerClient";
    /// <summary>Access token 效期。token 只放在前端記憶體，過期後以 refresh cookie 換發。</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Refresh token（登入狀態）效期。</summary>
    public int RefreshTokenDays { get; set; } = 14;
}
