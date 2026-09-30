using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonalManager.Api.Data;

namespace PersonalManager.Api.Services;

public class DbHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _services;

    public DbHealthCheck(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection OK")
                : HealthCheckResult.Unhealthy("Cannot connect to database");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check failed", ex);
        }
    }
}
