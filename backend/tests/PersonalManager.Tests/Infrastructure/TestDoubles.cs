using System.Collections.Concurrent;
using PersonalManager.Api.Services;

namespace PersonalManager.Tests.Infrastructure;

/// <summary>取代真正的寄信服務，讓測試可以讀取寄出的信件內容。</summary>
public sealed class TestEmailOutbox : IEmailService
{
    public sealed record Message(string To, string Subject, string HtmlBody);

    private readonly ConcurrentQueue<Message> _messages = new();

    public IReadOnlyList<Message> SentTo(string address) => _messages.Where(m => m.To == address).ToList();

    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        _messages.Enqueue(new Message(to, subject, htmlBody));
        return Task.CompletedTask;
    }
}

/// <summary>可以快轉的時鐘；預設為真實時間，測試過期行為時再往後調。</summary>
public sealed class AdjustableClock : TimeProvider
{
    private TimeSpan _offset = TimeSpan.Zero;

    public void Advance(TimeSpan by) => _offset += by;

    public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + _offset;
}
