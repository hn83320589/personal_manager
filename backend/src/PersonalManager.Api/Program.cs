using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using PersonalManager.Api.Auth;
using PersonalManager.Api.Data;
using PersonalManager.Api.Middleware;
using PersonalManager.Api.Services;
using PersonalManager.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Personal Manager API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
var emailConfig = builder.Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
if (emailConfig.IsConfigured)
{
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
    Console.WriteLine("Email: SMTP 模式");
}
else
{
    builder.Services.AddScoped<IEmailService, NoOpEmailService>();
    Console.WriteLine("Email: NoOp 模式（重設連結將輸出至 log）");
}

// FileStorage settings
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));

// File storage provider: S3-compatible (production) or local disk (dev)
var s3Config = builder.Configuration.GetSection("FileStorage:S3").Get<S3StorageSettings>();
if (s3Config?.IsConfigured == true)
{
    builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
        new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey),
        new AmazonS3Config
        {
            ServiceURL = s3Config.ServiceUrl,
            ForcePathStyle = s3Config.ForcePathStyle,
        }
    ));
    builder.Services.AddScoped<IFileStorageProvider, S3FileStorageProvider>();
    Console.WriteLine("檔案儲存: S3-compatible Object Storage");
}
else
{
    builder.Services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
    Console.WriteLine("檔案儲存: 本地磁碟（dev fallback）");
}

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// Rate Limiting — .NET 7+ built-in
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Auth endpoints (login / register): 10 requests per minute per IP
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Public write endpoints (guestbook POST): 10 requests per minute per IP
    options.AddPolicy("public_write", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Database（ADR-008：預設 SQLite，可由 Database:Provider 切換為 MySql）
builder.Services.AddPersistence(builder.Configuration, builder.Environment);

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IWorkExperienceService, WorkExperienceService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<ITodoItemService, TodoItemService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IWorkTaskService, WorkTaskService>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();
builder.Services.AddScoped<IGuestBookEntryService, GuestBookEntryService>();
builder.Services.AddScoped<IContactMethodService, ContactMethodService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IPortfolioAttachmentService, PortfolioAttachmentService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"))
    .AddCheck<DbHealthCheck>("database");

var app = builder.Build();

// Startup: 套用 migration 並建立初始資料
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await DatabaseSeeder.CreateIndexesAsync(db);
    await DatabaseSeeder.SeedAsync(db);
    app.Logger.LogInformation("資料庫 provider: {Provider}", db.Database.ProviderName);
}

// Middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// Static files for uploaded content
var fileStoragePath = Path.Combine(app.Environment.ContentRootPath,
    app.Configuration["FileStorage:RootPath"] ?? "files");
Directory.CreateDirectory(fileStoragePath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fileStoragePath),
    RequestPath = "/files"
});

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await ctx.Response.WriteAsync(result);
    }
});

app.Run();
public partial class Program { }
