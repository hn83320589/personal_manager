using System.Security.Claims;

namespace PersonalManager.Api.Common;

/// <summary>目前發出請求的使用者。service 以此判斷資料擁有者，不需要依賴 controller。</summary>
public interface ICurrentUser
{
    /// <summary>未登入時為 null。</summary>
    int? UserId { get; }

    bool IsAdmin { get; }

    /// <summary>取得登入者 ID；未登入時丟出 <see cref="UnauthenticatedException"/>。</summary>
    int RequireUserId();
}

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public int? UserId =>
        int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public bool IsAdmin => Principal?.IsInRole(Models.Roles.Admin) == true;

    public int RequireUserId() => UserId ?? throw new UnauthenticatedException();
}
