using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

/// <summary>
/// 一筆工作時間紀錄。連到任務時，任務與專案名稱由關聯取得，不另外複製一份；
/// 沒有連到任務的紀錄以 <see cref="Title"/> 描述做了什麼。
/// </summary>
public class TimeEntry : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? WorkTaskId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    /// <summary>有起訖時間時由兩者計算，否則由使用者直接填寫。</summary>
    public int DurationMinutes { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
