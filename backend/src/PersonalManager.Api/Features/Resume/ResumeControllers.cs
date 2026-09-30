using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.Common;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Features.Resume;

[ApiController]
[AllowAnonymous]
[Route("api/public/users/{username}")]
public sealed class PublicResumeController(EducationService educations, WorkExperienceService experiences) : ControllerBase
{
    [HttpGet("educations")]
    public async Task<ApiResponse<List<PublicEducationDto>>> Educations(string username) =>
        ApiResponse<List<PublicEducationDto>>.Ok(await educations.GetPublicAsync(username));

    [HttpGet("work-experiences")]
    public async Task<ApiResponse<List<PublicWorkExperienceDto>>> WorkExperiences(string username) =>
        ApiResponse<List<PublicWorkExperienceDto>>.Ok(await experiences.GetPublicAsync(username));
}

[ApiController]
[Authorize]
[Route("api/me/educations")]
public sealed class MyEducationsController(EducationService educations) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<EducationDto>>> List() =>
        ApiResponse<List<EducationDto>>.Ok(await educations.GetMineAsync());

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EducationDto>>> Create(SaveEducationRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<EducationDto>.Ok(await educations.CreateAsync(request), "已新增學歷"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<EducationDto>> Update(int id, SaveEducationRequest request) =>
        ApiResponse<EducationDto>.Ok(await educations.UpdateAsync(id, request), "已更新學歷");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await educations.DeleteAsync(id);
        return ApiResponse.Ok("已刪除學歷");
    }

    [HttpPut("order")]
    public async Task<ApiResponse> Reorder(ReorderRequest request)
    {
        await educations.ReorderAsync(request);
        return ApiResponse.Ok("已更新排序");
    }
}

[ApiController]
[Authorize]
[Route("api/me/work-experiences")]
public sealed class MyWorkExperiencesController(WorkExperienceService experiences) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<WorkExperienceDto>>> List() =>
        ApiResponse<List<WorkExperienceDto>>.Ok(await experiences.GetMineAsync());

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> Create(SaveWorkExperienceRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<WorkExperienceDto>.Ok(await experiences.CreateAsync(request), "已新增經歷"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<WorkExperienceDto>> Update(int id, SaveWorkExperienceRequest request) =>
        ApiResponse<WorkExperienceDto>.Ok(await experiences.UpdateAsync(id, request), "已更新經歷");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await experiences.DeleteAsync(id);
        return ApiResponse.Ok("已刪除經歷");
    }

    [HttpPut("order")]
    public async Task<ApiResponse> Reorder(ReorderRequest request)
    {
        await experiences.ReorderAsync(request);
        return ApiResponse.Ok("已更新排序");
    }
}
