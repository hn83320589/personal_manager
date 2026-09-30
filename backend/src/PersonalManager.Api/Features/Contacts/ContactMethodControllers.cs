using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.Common;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Features.Contacts;

[ApiController]
[AllowAnonymous]
[Route("api/public/users/{username}/contact-methods")]
public sealed class PublicContactMethodsController(ContactMethodService contacts) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<PublicContactMethodDto>>> List(string username) =>
        ApiResponse<List<PublicContactMethodDto>>.Ok(await contacts.GetPublicAsync(username));
}

[ApiController]
[Authorize]
[Route("api/me/contact-methods")]
public sealed class MyContactMethodsController(ContactMethodService contacts) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<ContactMethodDto>>> List() =>
        ApiResponse<List<ContactMethodDto>>.Ok(await contacts.GetMineAsync());

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ContactMethodDto>>> Create(SaveContactMethodRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<ContactMethodDto>.Ok(await contacts.CreateAsync(request), "已新增聯絡方式"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<ContactMethodDto>> Update(int id, SaveContactMethodRequest request) =>
        ApiResponse<ContactMethodDto>.Ok(await contacts.UpdateAsync(id, request), "已更新聯絡方式");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await contacts.DeleteAsync(id);
        return ApiResponse.Ok("已刪除聯絡方式");
    }

    [HttpPut("order")]
    public async Task<ApiResponse> Reorder(ReorderRequest request)
    {
        await contacts.ReorderAsync(request);
        return ApiResponse.Ok("已更新排序");
    }
}
