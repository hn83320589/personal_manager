using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Common;

public static class Reordering
{
    /// <summary>新項目排在最後。</summary>
    public static async Task<int> NextPositionAsync<T>(this IQueryable<T> ownedItems) where T : ISortable =>
        (await ownedItems.MaxAsync(x => (int?)x.SortOrder) ?? 0) + 1;

    /// <summary>
    /// 依 <paramref name="request"/> 的順序重新編號。清單必須剛好是使用者的全部項目且不重複，
    /// 避免漏送或夾帶別人的 ID 造成排序錯亂。
    /// </summary>
    public static void ApplyOrder<T>(this IReadOnlyCollection<T> ownedItems, ReorderRequest request) where T : ISortable
    {
        var position = request.Ids.Distinct().Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index + 1);
        var isCompleteAndUnique = position.Count == request.Ids.Count
                                  && ownedItems.Select(i => i.Id).ToHashSet().SetEquals(position.Keys);
        if (!isCompleteAndUnique)
            throw new DomainValidationException("排序清單必須剛好包含你所有的項目，且不可重複");

        foreach (var item in ownedItems)
            item.SortOrder = position[item.Id];
    }

    /// <summary>載入使用者的全部項目並依 <paramref name="request"/> 重新編號；呼叫端負責存檔。</summary>
    public static async Task ApplyOrderAsync<T>(this IQueryable<T> ownedItems, ReorderRequest request) where T : class, ISortable =>
        (await ownedItems.ToListAsync()).ApplyOrder(request);
}
