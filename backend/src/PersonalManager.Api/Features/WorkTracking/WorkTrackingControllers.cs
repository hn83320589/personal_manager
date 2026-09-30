using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.Common;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.WorkTracking;

[ApiController]
[Authorize]
[Route("api/me/projects")]
public sealed class MyProjectsController(ProjectService projects) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<ProjectDto>>> List() => ApiResponse<List<ProjectDto>>.Ok(await projects.GetMineAsync());

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create(SaveProjectRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<ProjectDto>.Ok(await projects.CreateAsync(request), "已新增專案"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<ProjectDto>> Update(int id, SaveProjectRequest request) =>
        ApiResponse<ProjectDto>.Ok(await projects.UpdateAsync(id, request), "已更新專案");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await projects.DeleteAsync(id);
        return ApiResponse.Ok("已刪除專案，其下任務已改為未分類");
    }

    [HttpPut("order")]
    public async Task<ApiResponse> Reorder(ReorderRequest request)
    {
        await projects.ReorderAsync(request);
        return ApiResponse.Ok("已更新排序");
    }
}

[ApiController]
[Authorize]
[Route("api/me/work-tasks")]
public sealed class MyWorkTasksController(WorkTaskService tasks) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<WorkTaskDto>>> List([FromQuery] int? projectId, [FromQuery] WorkTaskStatus? status) =>
        ApiResponse<List<WorkTaskDto>>.Ok(await tasks.GetMineAsync(projectId, status));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkTaskDto>>> Create(SaveWorkTaskRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<WorkTaskDto>.Ok(await tasks.CreateAsync(request), "已新增任務"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<WorkTaskDto>> Update(int id, SaveWorkTaskRequest request) =>
        ApiResponse<WorkTaskDto>.Ok(await tasks.UpdateAsync(id, request), "已更新任務");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await tasks.DeleteAsync(id);
        return ApiResponse.Ok("已刪除任務");
    }
}

[ApiController]
[Authorize]
[Route("api/me/time-entries")]
public sealed class MyTimeEntriesController(TimeEntryService entries) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<TimeEntryDto>>> List([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int? workTaskId) =>
        ApiResponse<List<TimeEntryDto>>.Ok(await entries.GetMineAsync(from, to, workTaskId));

    [HttpGet("summary")]
    public async Task<ApiResponse<TimeSummaryDto>> Summary([FromQuery] DateOnly? from, [FromQuery] DateOnly? to) =>
        ApiResponse<TimeSummaryDto>.Ok(await entries.GetSummaryAsync(from, to));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> Create(SaveTimeEntryRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<TimeEntryDto>.Ok(await entries.CreateAsync(request), "已新增時間紀錄"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<TimeEntryDto>> Update(int id, SaveTimeEntryRequest request) =>
        ApiResponse<TimeEntryDto>.Ok(await entries.UpdateAsync(id, request), "已更新時間紀錄");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await entries.DeleteAsync(id);
        return ApiResponse.Ok("已刪除時間紀錄");
    }
}
