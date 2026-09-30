using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.Common;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Features.Portfolios;

/// <summary>公開頁面：只回傳設為公開的作品。列表為卡片（不含內容區塊），單件作品以 slug 取得。</summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/users/{username}/portfolios")]
public sealed class PublicPortfoliosController(PortfolioService portfolios) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<PortfolioCardDto>>> List(string username,
        [FromQuery] string? category, [FromQuery] string? tag) =>
        ApiResponse<List<PortfolioCardDto>>.Ok(await portfolios.GetPublicCardsAsync(username, category, tag));

    [HttpGet("facets")]
    public async Task<ApiResponse<PortfolioFacetsDto>> Facets(string username) =>
        ApiResponse<PortfolioFacetsDto>.Ok(await portfolios.GetFacetsAsync(username));

    [HttpGet("{slug}")]
    public async Task<ApiResponse<PublicPortfolioDto>> Get(string username, string slug) =>
        ApiResponse<PublicPortfolioDto>.Ok(await portfolios.GetPublicAsync(username, slug));
}

[ApiController]
[Authorize]
[Route("api/me/portfolios")]
public sealed class MyPortfoliosController(PortfolioService portfolios) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<PortfolioSummaryDto>>> List() =>
        ApiResponse<List<PortfolioSummaryDto>>.Ok(await portfolios.GetMineAsync());

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<PortfolioDto>> Get(int id) =>
        ApiResponse<PortfolioDto>.Ok(await portfolios.GetMineAsync(id));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PortfolioDto>>> Create(SavePortfolioRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<PortfolioDto>.Ok(await portfolios.CreateAsync(request), "已建立作品"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<PortfolioDto>> Update(int id, SavePortfolioRequest request) =>
        ApiResponse<PortfolioDto>.Ok(await portfolios.UpdateAsync(id, request), "已儲存作品");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await portfolios.DeleteAsync(id);
        return ApiResponse.Ok("已刪除作品");
    }

    [HttpPut("order")]
    public async Task<ApiResponse> Reorder(ReorderRequest request)
    {
        await portfolios.ReorderAsync(request);
        return ApiResponse.Ok("已更新排序");
    }
}
