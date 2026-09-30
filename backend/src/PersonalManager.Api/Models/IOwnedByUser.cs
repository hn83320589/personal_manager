namespace PersonalManager.Api.Models;

/// <summary>屬於某位使用者的資料。後台（/api/me）的查詢一律以 <c>OwnedBy</c> 限定範圍。</summary>
public interface IOwnedByUser
{
    int UserId { get; }
}
