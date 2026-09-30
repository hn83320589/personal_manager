using PersonalManager.Api.Common;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Middleware;

/// <summary>
/// 將 <see cref="AppException"/> 轉成對應的狀態碼與訊息；其他例外一律回傳 500 與通用訊息，
/// 詳細內容只寫進 log，避免 SQL、路徑等內部資訊外洩給使用者。
/// </summary>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            var errors = ex is DomainValidationException validation ? validation.Errors.ToList() : null;
            await WriteError(context, ex.StatusCode, ApiResponse.Fail(ex.Message, errors));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // 用戶端已中斷連線，沒有人會收到回應
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "處理 {Method} {Path} 時發生未預期的錯誤", context.Request.Method, context.Request.Path);
            await WriteError(context, StatusCodes.Status500InternalServerError, ApiResponse.Fail("系統發生錯誤，請稍後再試"));
        }
    }

    private static async Task WriteError(HttpContext context, int statusCode, ApiResponse body)
    {
        if (context.Response.HasStarted)
            return;
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(body);
    }
}
