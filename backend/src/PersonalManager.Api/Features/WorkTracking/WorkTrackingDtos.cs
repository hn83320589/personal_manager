using System.ComponentModel.DataAnnotations;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.WorkTracking;

public sealed record SaveProjectRequest(
    [Required(ErrorMessage = "請輸入專案名稱"), StringLength(100)] string Name,
    [StringLength(2000)] string? Description,
    [RegularExpression("^(#[0-9a-fA-F]{6})?$", ErrorMessage = "顏色需為 #RRGGBB 格式")] string? Color);

public sealed record ProjectDto(int Id, string Name, string Description, string Color, int SortOrder);

public sealed record SaveWorkTaskRequest(
    [Required(ErrorMessage = "請輸入任務名稱"), StringLength(200)] string Title,
    [StringLength(5000)] string? Description,
    int? ProjectId,
    WorkTaskPriority Priority,
    WorkTaskStatus Status,
    [Range(0, 10_000)] double EstimatedHours,
    DateTime? DueDate);

/// <summary><see cref="ActualMinutes"/> 為此任務所有時間紀錄的加總。</summary>
public sealed record WorkTaskDto(
    int Id, string Title, string Description, int? ProjectId, string? ProjectName, WorkTaskPriority Priority,
    WorkTaskStatus Status, double EstimatedHours, int ActualMinutes, DateTime? DueDate, DateTime? CompletedAt);

/// <summary>連到任務時 <see cref="Title"/> 可留空；有起訖時間時時長由兩者計算，否則必須填寫時長。</summary>
public sealed record SaveTimeEntryRequest(
    int? WorkTaskId,
    [StringLength(200)] string? Title,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    [Range(1, 1440)] int? DurationMinutes,
    [StringLength(2000)] string? Description) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (StartTime is not null && EndTime is not null && EndTime <= StartTime)
            yield return new ValidationResult("結束時間必須晚於開始時間", [nameof(EndTime)]);
        if ((StartTime is null || EndTime is null) && DurationMinutes is null)
            yield return new ValidationResult("請填寫起訖時間或時長", [nameof(DurationMinutes)]);
        if (WorkTaskId is null && string.IsNullOrWhiteSpace(Title))
            yield return new ValidationResult("沒有連結任務時請描述做了什麼", [nameof(Title)]);
    }

    public int ResolveDurationMinutes() =>
        StartTime is { } start && EndTime is { } end ? (int)(end - start).TotalMinutes : DurationMinutes!.Value;
}

public sealed record TimeEntryDto(
    int Id, int? WorkTaskId, string? WorkTaskTitle, string? ProjectName, string Title, DateOnly Date,
    TimeOnly? StartTime, TimeOnly? EndTime, int DurationMinutes, string Description);

public sealed record ProjectTotalDto(int? ProjectId, string? ProjectName, int Minutes);

public sealed record DailyTotalDto(DateOnly Date, int Minutes);

public sealed record TimeSummaryDto(int TotalMinutes, IReadOnlyList<ProjectTotalDto> ByProject, IReadOnlyList<DailyTotalDto> ByDay);
