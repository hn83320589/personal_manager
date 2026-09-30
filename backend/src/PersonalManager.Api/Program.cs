using PersonalManager.Api.Auth;
using PersonalManager.Api.Data;
using PersonalManager.Api.Setup;

var builder = WebApplication.CreateBuilder(args);

// 建置 host 之前還沒有正式的 logger，啟動階段的訊息用這個暫時的 logger 輸出
using var startupLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var startupLogger = startupLoggerFactory.CreateLogger("Startup");

builder.Services
    .AddApiControllers()
    .AddApiDocumentation()
    .AddFrontendCors(builder.Configuration, builder.Environment)
    .AddRateLimitPolicies()
    .AddApiHealthChecks()
    .AddJwtAuthentication(builder.Configuration, builder.Environment, startupLogger)
    .AddPersistence(builder.Configuration, builder.Environment)   // ADR-008：預設 SQLite
    .AddEmail(builder.Configuration, startupLogger)
    .AddFileStorage(builder.Configuration, startupLogger)
    .AddApplicationServices();

var app = builder.Build();

await app.InitializeDatabaseAsync();
app.UseApiPipeline();
app.Run();

public partial class Program { }
