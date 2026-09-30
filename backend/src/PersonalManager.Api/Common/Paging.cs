using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Common;

public static class Paging
{
    /// <summary>在資料庫端計算總數並取出指定頁；頁碼從 1 開始，每頁筆數限制在 1 到 <paramref name="maxPageSize"/>。</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize, int maxPageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, maxPageSize);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<T> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
