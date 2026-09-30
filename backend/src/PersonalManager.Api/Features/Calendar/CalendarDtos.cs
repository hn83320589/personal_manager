using System.ComponentModel.DataAnnotations;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Calendar;

public sealed record SaveEventRequest(
    [Required(ErrorMessage = "請輸入標題"), StringLength(200)] string Title,
    [StringLength(5000)] string? Description,
    DateTime StartTime,
    DateTime EndTime,
    bool IsAllDay,
    bool IsPublic,
    [RegularExpression("^(#[0-9a-fA-F]{6})?$", ErrorMessage = "顏色需為 #RRGGBB 格式")] string? Color,
    Recurrence Recurrence,
    DateOnly? RecurrenceUntil) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EndTime < StartTime)
            yield return new ValidationResult("結束時間不能早於開始時間", [nameof(EndTime)]);
        if (RecurrenceUntil is not null && RecurrenceUntil < DateOnly.FromDateTime(StartTime))
            yield return new ValidationResult("重複結束日不能早於開始日", [nameof(RecurrenceUntil)]);
    }
}

public sealed record EventDto(
    int Id, string Title, string Description, DateTime StartTime, DateTime EndTime, bool IsAllDay, bool IsPublic,
    string Color, Recurrence Recurrence, DateOnly? RecurrenceUntil);

/// <summary>展開後的一次發生；重複事件的每一次都指向同一個 <see cref="EventId"/>。</summary>
public sealed record OccurrenceDto(
    int EventId, string Title, string Description, DateTime Start, DateTime End, bool IsAllDay, string Color, bool IsRecurring);
