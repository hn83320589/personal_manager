using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}

/// <summary>
/// Sends real emails via SMTP when Email settings are configured.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To} with subject: {Subject}", to, subject);
    }
}

/// <summary>
/// 未設定 SMTP 時使用。開發環境把信件內容（含重設密碼連結）寫進 log 方便測試；
/// 其他環境只記錄「沒有寄出」，避免把重設連結留在 log 中被他人取得。
/// </summary>
public class NoOpEmailService(ILogger<NoOpEmailService> logger, IHostEnvironment environment) : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (environment.IsDevelopment())
            logger.LogWarning("[未寄出的信件] To: {To} | Subject: {Subject} | Body: {Body}", to, subject, htmlBody);
        else
            logger.LogWarning("未設定 SMTP，信件「{Subject}」沒有寄出", subject);
        return Task.CompletedTask;
    }
}
