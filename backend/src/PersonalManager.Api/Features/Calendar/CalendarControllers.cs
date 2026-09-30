using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Features.Calendar;

[ApiController]
[AllowAnonymous]
[Route("api/public/users/{username}/calendar")]
public sealed class PublicCalendarController(CalendarService calendar) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<OccurrenceDto>>> Occurrences(string username, [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        ApiResponse<List<OccurrenceDto>>.Ok(await calendar.GetPublicOccurrencesAsync(username, from, to));
}

[ApiController]
[Authorize]
[Route("api/me/calendar")]
public sealed class MyCalendarController(CalendarService calendar) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<OccurrenceDto>>> Occurrences([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        ApiResponse<List<OccurrenceDto>>.Ok(await calendar.GetMyOccurrencesAsync(from, to));

    [HttpGet("events/{id:int}")]
    public async Task<ApiResponse<EventDto>> Get(int id) => ApiResponse<EventDto>.Ok(await calendar.GetMyEventAsync(id));

    [HttpPost("events")]
    public async Task<ActionResult<ApiResponse<EventDto>>> Create(SaveEventRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<EventDto>.Ok(await calendar.CreateAsync(request), "已新增行程"));

    [HttpPut("events/{id:int}")]
    public async Task<ApiResponse<EventDto>> Update(int id, SaveEventRequest request) =>
        ApiResponse<EventDto>.Ok(await calendar.UpdateAsync(id, request), "已更新行程");

    [HttpDelete("events/{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await calendar.DeleteAsync(id);
        return ApiResponse.Ok("已刪除行程");
    }
}
