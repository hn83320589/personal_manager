namespace PersonalManager.Api.Common;

/// <summary>
/// 業務上預期會發生的錯誤。訊息由我們撰寫、可安全顯示給使用者，
/// 由 <see cref="Middleware.ErrorHandlingMiddleware"/> 轉成對應的 HTTP 狀態碼。
/// 其他任何例外都視為非預期錯誤，只回傳通用訊息。
/// </summary>
public abstract class AppException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }
}

public sealed class NotFoundException(string message = "找不到指定的資料") : AppException(message)
{
    public override int StatusCode => StatusCodes.Status404NotFound;
}

public sealed class ForbiddenException(string message = "沒有權限執行這個操作") : AppException(message)
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
}

public sealed class UnauthenticatedException(string message = "請先登入") : AppException(message)
{
    public override int StatusCode => StatusCodes.Status401Unauthorized;
}

public sealed class ConflictException(string message) : AppException(message)
{
    public override int StatusCode => StatusCodes.Status409Conflict;
}

public sealed class DomainValidationException(string message, IReadOnlyList<string>? errors = null) : AppException(message)
{
    public IReadOnlyList<string> Errors { get; } = errors ?? [];
    public override int StatusCode => StatusCodes.Status400BadRequest;
}
