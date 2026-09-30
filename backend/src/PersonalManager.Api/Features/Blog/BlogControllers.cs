using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Setup;

namespace PersonalManager.Api.Features.Blog;

[ApiController]
[AllowAnonymous]
[Route("api/public/users/{username}/posts")]
public sealed class PublicPostsController(BlogService blog) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<PagedResult<PublicPostSummaryDto>>> List(string username,
        [FromQuery] string? q, [FromQuery] string? tag, [FromQuery] string? category,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        ApiResponse<PagedResult<PublicPostSummaryDto>>.Ok(await blog.GetPublishedAsync(username, q, tag, category, page, pageSize));

    [HttpGet("facets")]
    public async Task<ApiResponse<PostFacetsDto>> Facets(string username) =>
        ApiResponse<PostFacetsDto>.Ok(await blog.GetFacetsAsync(username));

    [HttpGet("{slug}")]
    public async Task<ApiResponse<PublicPostDto>> Get(string username, string slug) =>
        ApiResponse<PublicPostDto>.Ok(await blog.GetPublishedPostAsync(username, slug));

    [HttpPost("{slug}/views")]
    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]
    public async Task<ApiResponse> RecordView(string username, string slug)
    {
        await blog.RecordViewAsync(username, slug);
        return ApiResponse.Ok();
    }
}

[ApiController]
[Authorize]
[Route("api/me/posts")]
public sealed class MyPostsController(BlogService blog) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<PagedResult<MyPostSummaryDto>>> List(
        [FromQuery] string? q, [FromQuery] BlogPostStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        ApiResponse<PagedResult<MyPostSummaryDto>>.Ok(await blog.GetMineAsync(q, status, page, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<MyPostDto>> Get(int id) =>
        ApiResponse<MyPostDto>.Ok(await blog.GetMyPostAsync(id));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MyPostDto>>> Create(SavePostRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<MyPostDto>.Ok(await blog.CreateAsync(request), "已建立文章"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<MyPostDto>> Update(int id, SavePostRequest request) =>
        ApiResponse<MyPostDto>.Ok(await blog.UpdateAsync(id, request), "已儲存文章");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await blog.DeleteAsync(id);
        return ApiResponse.Ok("已刪除文章");
    }
}
