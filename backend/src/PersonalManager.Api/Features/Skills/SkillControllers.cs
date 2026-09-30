using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.Common;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Features.Skills;

/// <summary>公開頁面：只回傳該使用者設為公開的技能。</summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/users/{username}/skills")]
public sealed class PublicSkillsController(SkillService skills) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<PublicSkillDto>>> List(string username) =>
        ApiResponse<List<PublicSkillDto>>.Ok(await skills.GetPublicAsync(username));
}

/// <summary>後台：目前登入者管理自己的技能。</summary>
[ApiController]
[Authorize]
[Route("api/me/skills")]
public sealed class MySkillsController(SkillService skills) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<SkillDto>>> List() =>
        ApiResponse<List<SkillDto>>.Ok(await skills.GetMineAsync());

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SkillDto>>> Create(SaveSkillRequest request)
    {
        var created = await skills.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SkillDto>.Ok(created, "已新增技能"));
    }

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<SkillDto>> Update(int id, SaveSkillRequest request) =>
        ApiResponse<SkillDto>.Ok(await skills.UpdateAsync(id, request), "已更新技能");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await skills.DeleteAsync(id);
        return ApiResponse.Ok("已刪除技能");
    }

    [HttpPut("order")]
    public async Task<ApiResponse> Reorder(ReorderRequest request)
    {
        await skills.ReorderAsync(request);
        return ApiResponse.Ok("已更新排序");
    }
}
