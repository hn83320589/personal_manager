namespace PersonalManager.Api.Settings;

public class AdminSettings
{
    /// <summary>
    /// 以這些 Email 註冊的帳號直接成為管理員，用來建立網站的第一位管理員
    /// （正式環境不會執行示範資料 seeder）。以環境變數 <c>Admin__BootstrapEmails__0</c> 設定。
    /// </summary>
    public string[] BootstrapEmails { get; set; } = [];
}
