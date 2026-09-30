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
    public int ExpiryHours { get; set; } = 24;
}
