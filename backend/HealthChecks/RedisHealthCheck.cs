using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BibliotecaAPI.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RedisHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy("String de conexão do Redis não configurada.");
        }

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 3000;
            options.SyncTimeout = 3000;
            options.AbortOnConnectFail = false;

            using var redis = await ConnectionMultiplexer.ConnectAsync(options);
            if (!redis.IsConnected)
            {
                return HealthCheckResult.Unhealthy("Não foi possível estabelecer conexão com o Redis.");
            }

            var db = redis.GetDatabase();
            var ping = await db.PingAsync();
            return HealthCheckResult.Healthy($"Redis operacional. Latência de Ping: {ping.TotalMilliseconds:F1}ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Falha ao verificar saúde do Redis: {ex.Message}", ex);
        }
    }
}
