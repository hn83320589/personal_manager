using System.ComponentModel.DataAnnotations;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Skills;

/// <summary>新增與更新共用。熟練度與年資選填，讓不習慣標示等級的職業也能使用。</summary>
public sealed record SaveSkillRequest(
    [Required(ErrorMessage = "請輸入技能名稱"), StringLength(100)] string Name,
    [StringLength(50)] string? Category,
    SkillLevel? Level,
    [Range(0, 80)] int? YearsOfExperience,
    bool IsPublic = true);

/// <summary>後台使用，包含公開狀態與排序。</summary>
public sealed record SkillDto(
    int Id, string Name, string Category, SkillLevel? Level, int? YearsOfExperience, bool IsPublic, int SortOrder);

/// <summary>公開頁面使用，不含管理用欄位。</summary>
public sealed record PublicSkillDto(int Id, string Name, string Category, SkillLevel? Level, int? YearsOfExperience);
