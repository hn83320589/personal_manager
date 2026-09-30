using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Setup;

public static class RateLimitPolicies
{
    /// <summary>登入、註冊、重設密碼等認證端點。</summary>
    public const string Auth = "auth";

    /// <summary>
    /// 還原與結束工作階段（refresh、logout）。前端每次開啟頁面都會 refresh，額度需寬鬆些，
    /// 也不能與登入共用額度，否則多開分頁就會被登出。refresh token 為 256 位元亂數，無法猜測。
    /// </summary>
    public const string Session = "session";

    /// <summary>不需登入即可寫入的端點（留言板）。</summary>
    public const string PublicWrite = "public_write";
}

/// <summary>HTTP 層的服務註冊：controller、JSON 格式、API 文件、CORS、流量限制、健康檢查。</summary>
public static class ApiSetup
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                // 作品區塊以「type」區分類型；允許 type 不是第一個屬性
                o.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
            })
            // 驗證失敗也回傳與其他錯誤相同的 ApiResponse 格式，前端只需處理一種錯誤結構
            .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "欄位格式不正確" : e.ErrorMessage)
                    .ToList();
                return new BadRequestObjectResult(ApiResponse.Fail("資料格式有誤", errors));
            });
        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Personal Manager API", Version = "v1" });
            // 讓產生的前端型別（openapi-typescript）與 C# 一致：非 nullable 的屬性為必填，
            // 作品區塊依 [JsonDerivedType] 產生以 type 區分的 oneOf
            c.SupportNonNullableReferenceTypes();
            c.NonNullableReferenceTypesAsRequired();
            c.UseOneOfForPolymorphism();
            c.UseAllOfForInheritance();
            c.SelectDiscriminatorNameUsing(type =>
                type.GetCustomAttribute<JsonPolymorphicAttribute>()?.TypeDiscriminatorPropertyName);
            c.SelectDiscriminatorValueUsing(subType =>
                subType.BaseType?.GetCustomAttributes<JsonDerivedTypeAttribute>()
                    .FirstOrDefault(a => a.DerivedType == subType)?.TypeDiscriminator as string);
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

    /// <summary>每個 IP 每分鐘的次數上限，可由 <c>RateLimiting:*PermitsPerMinute</c> 調整（預設 10；工作階段 60）。</summary>
    public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        var authLimit = configuration.GetValue("RateLimiting:AuthPermitsPerMinute", 10);
        var publicWriteLimit = configuration.GetValue("RateLimiting:PublicWritePermitsPerMinute", 10);
        var sessionLimit = configuration.GetValue("RateLimiting:SessionPermitsPerMinute", 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicies.Auth, PerClientIp(authLimit));
            options.AddPolicy(RateLimitPolicies.PublicWrite, PerClientIp(publicWriteLimit));
            options.AddPolicy(RateLimitPolicies.Session, PerClientIp(sessionLimit));
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
