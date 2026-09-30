using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Setup;

namespace PersonalManager.Api.Features.Guestbook;

[ApiController]
[AllowAnonymous]
[Route("api/public/users/{username}/guestbook")]
public sealed class PublicGuestbookController(GuestbookService guestbook) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<PagedResult<PublicGuestbookEntryDto>>> List(string username,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        ApiResponse<PagedResult<PublicGuestbookEntryDto>>.Ok(await guestbook.GetApprovedAsync(username, page, pageSize));

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]
    public async Task<ActionResult<ApiResponse>> Leave(string username, LeaveMessageRequest request)
    {
        await guestbook.LeaveAsync(username, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok("留言已送出，版主審核後就會顯示"));
    }
}

[ApiController]
[Authorize]
[Route("api/me/guestbook")]
public sealed class MyGuestbookController(GuestbookService guestbook) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<PagedResult<GuestbookEntryDto>>> List(
        [FromQuery] GuestbookFilter status = GuestbookFilter.All, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        ApiResponse<PagedResult<GuestbookEntryDto>>.Ok(await guestbook.GetMineAsync(status, page, pageSize));

    [HttpPut("{id:int}/approval")]
    public async Task<ApiResponse> SetApproval(int id, SetApprovalRequest request)
    {
        await guestbook.SetApprovalAsync(id, request.IsApproved);
        return ApiResponse.Ok(request.IsApproved ? "已公開留言" : "已隱藏留言");
    }

    [HttpPut("{id:int}/reply")]
    public async Task<ApiResponse> Reply(int id, ReplyRequest request)
    {
        await guestbook.ReplyAsync(id, request.Reply);
        return ApiResponse.Ok("已儲存回覆");
    }

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await guestbook.DeleteAsync(id);
        return ApiResponse.Ok("已刪除留言");
    }
}
