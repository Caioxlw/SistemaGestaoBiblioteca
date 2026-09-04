using BibliotecaAPI.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BibliotecaAPI.HealthChecks;

public class PostgresHealthCheck : IHealthCheck
{
    private readonly BibliotecaDbContext _context;

    public PostgresHealthCheck(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                return HealthCheckResult.Healthy("PostgreSQL está conectado e operacional.");
            }
            return HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados PostgreSQL.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Falha ao verificar saúde do PostgreSQL: {ex.Message}", ex);
        }
    }
}
