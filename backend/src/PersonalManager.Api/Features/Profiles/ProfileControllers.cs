using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Features.Profiles;

/// <summary>使用者目錄與公開個人資料。</summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/users")]
public sealed class PublicProfilesController(ProfileService profiles) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<PagedResult<DirectoryCardDto>>> Directory(
        [FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 24) =>
        ApiResponse<PagedResult<DirectoryCardDto>>.Ok(await profiles.GetDirectoryAsync(q, page, pageSize));

    [HttpGet("{username}")]
    public async Task<ApiResponse<ProfileDto>> Get(string username) =>
        ApiResponse<ProfileDto>.Ok(await profiles.GetPublicAsync(username));
}

[ApiController]
[Authorize]
[Route("api/me/profile")]
public sealed class MyProfileController(ProfileService profiles) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<ProfileDto>> Get() =>
        ApiResponse<ProfileDto>.Ok(await profiles.GetMineAsync());

    [HttpPut]
    public async Task<ApiResponse<ProfileDto>> Save(SaveProfileRequest request) =>
        ApiResponse<ProfileDto>.Ok(await profiles.SaveAsync(request), "已儲存個人資料");
}
