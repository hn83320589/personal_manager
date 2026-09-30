using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PersonalManager.Api.Data;
using PersonalManager.Api.Middleware;

namespace PersonalManager.Api.Setup;

public static class PipelineSetup
{
    /// <summary>套用 migration；Development 另外建立示範資料（含已知密碼的帳號，只能用於開發）。</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        if (app.Environment.IsDevelopment())
            await DatabaseSeeder.SeedAsync(db);
        app.Logger.LogInformation("資料庫 provider：{Provider}", db.Database.ProviderName);
    }

    /// <summary>Middleware 順序：錯誤處理最外層，CORS 在驗證之前，流量限制在驗證之前以擋下暴力嘗試。</summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseUploadedFiles();
        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/api/health", new HealthCheckOptions { ResponseWriter = WriteHealthJson });
        return app;
    }

    private static void UseUploadedFiles(this WebApplication app)
    {
        var root = Path.Combine(app.Environment.ContentRootPath, app.Configuration["FileStorage:RootPath"] ?? "files");
        Directory.CreateDirectory(root);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = "/files",
            // 使用者上傳的內容：禁止瀏覽器自行猜測類型，避免被當成 HTML 執行
            OnPrepareResponse = context => context.Context.Response.Headers.XContentTypeOptions = "nosniff"
        });
    }

    private static Task WriteHealthJson(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        }));
    }
}
