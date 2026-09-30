using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Files;

[ApiController]
[Authorize]
[Route("api/me/files")]
public sealed class MyFilesController(FileService files) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<PagedResult<FileDto>>> List(
        [FromQuery] FileKind? kind, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        ApiResponse<PagedResult<FileDto>>.Ok(await files.GetMineAsync(kind, page, pageSize));

    [HttpPost]
    [RequestSizeLimit(FileService.RequestSizeCeiling)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileService.RequestSizeCeiling)]
    public async Task<ActionResult<ApiResponse<FileDto>>> Upload(IFormFile? file) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<FileDto>.Ok(await files.UploadAsync(file), "已上傳檔案"));

    [HttpGet("{id:int}/usages")]
    public async Task<ApiResponse<List<FileUsageDto>>> Usages(int id) =>
        ApiResponse<List<FileUsageDto>>.Ok(await files.GetUsagesAsync(id));

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await files.DeleteAsync(id);
        return ApiResponse.Ok("已刪除檔案");
    }
}
