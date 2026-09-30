using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Features.Resume;

public sealed record SaveEducationRequest(
    [Required(ErrorMessage = "請輸入學校名稱"), StringLength(200)] string School,
    [StringLength(100)] string? Degree,
    [StringLength(200)] string? FieldOfStudy,
    [Range(1900, 2100)] int? StartYear,
    [Range(1900, 2100)] int? EndYear,
    [StringLength(5000)] string? Description,
    bool IsPublic = true) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (StartYear is not null && EndYear is not null && EndYear < StartYear)
            yield return new ValidationResult("畢業年份不能早於入學年份", [nameof(EndYear)]);
    }
}

public sealed record EducationDto(
    int Id, string School, string Degree, string FieldOfStudy, int? StartYear, int? EndYear, string Description,
    bool IsPublic, int SortOrder);

public sealed record PublicEducationDto(
    int Id, string School, string Degree, string FieldOfStudy, int? StartYear, int? EndYear, string Description);

/// <summary>在職中（<see cref="IsCurrent"/>）時忽略結束日期。</summary>
public sealed record SaveWorkExperienceRequest(
    [Required(ErrorMessage = "請輸入公司或組織名稱"), StringLength(200)] string Company,
    [Required(ErrorMessage = "請輸入職稱"), StringLength(200)] string Position,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    [StringLength(5000)] string? Description,
    bool IsPublic = true) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (!IsCurrent && StartDate is not null && EndDate is not null && EndDate < StartDate)
            yield return new ValidationResult("結束日期不能早於開始日期", [nameof(EndDate)]);
    }
}

public sealed record WorkExperienceDto(
    int Id, string Company, string Position, DateOnly? StartDate, DateOnly? EndDate, bool IsCurrent, string Description,
    bool IsPublic, int SortOrder);

public sealed record PublicWorkExperienceDto(
    int Id, string Company, string Position, DateOnly? StartDate, DateOnly? EndDate, bool IsCurrent, string Description);
