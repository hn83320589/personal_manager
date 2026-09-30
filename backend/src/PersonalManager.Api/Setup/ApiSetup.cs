using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Setup;

public static class RateLimitPolicies
{
    /// <summary>登入、註冊、重設密碼等認證端點。</summary>
    public const string Auth = "auth";

    /// <summary>不需登入即可寫入的端點（留言板）。</summary>
    public const string PublicWrite = "public_write";
}

/// <summary>HTTP 層的服務註冊：controller、JSON 格式、API 文件、CORS、流量限制、健康檢查。</summary>
public static class ApiSetup
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Personal Manager API", Version = "v1" });
            var bearer = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            c.AddSecurityDefinition("Bearer", bearer);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearer, Array.Empty<string>() } });
        });
        return services;
    }

    /// <summary>
    /// 允許的前端來源由 <c>Cors:AllowedOrigins</c> 設定（環境變數 <c>Cors__AllowedOrigins__0</c>）。
    /// localhost 預設值只在 Development 且未設定時套用，不寫進 appsettings.json，
    /// 避免設定陣列依索引合併時殘留到正式環境。
    /// </summary>
    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        string[] origins = configured.Length > 0 || !environment.IsDevelopment()
            ? configured
            : ["http://localhost:5173", "http://localhost:4173"];

        services.AddCors(options => options.AddDefaultPolicy(policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
        return services;
    }

    public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicies.Auth, PerClientIp(permitsPerMinute: 10));
            options.AddPolicy(RateLimitPolicies.PublicWrite, PerClientIp(permitsPerMinute: 10));
        });
        return services;
    }

    private static Func<HttpContext, RateLimitPartition<string>> PerClientIp(int permitsPerMinute) =>
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitsPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });

    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
            .AddCheck<DbHealthCheck>("database");
        return services;
    }
}
