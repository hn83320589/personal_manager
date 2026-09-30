namespace PersonalManager.Api.Models;

/// <summary>使用者可以自行調整顯示順序的資料。</summary>
public interface ISortable
{
    int Id { get; }
    int SortOrder { get; set; }
}
