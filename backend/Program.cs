using BibliotecaAPI.Data;
using BibliotecaAPI.HealthChecks;
using BibliotecaAPI.Middlewares;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext com PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BibliotecaDbContext>(options => 
    options.UseNpgsql(connectionString));

// Redis Cache
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "SmartLib_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache(); // Fallback
}

// Configuração do JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "SmartLibSuperSecretKey2026!@#$%^&*()MinimoSegura32Chars";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartLib",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SmartLibUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Deep Health Checks (PostgreSQL e Redis)
builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgresql", tags: new[] { "db", "ready" })
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "cache", "ready" });

// Repositories
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<ILivroRepository, LivroRepository>();
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IEmprestimoRepository, EmprestimoRepository>();

// Services
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<ILivroService, LivroService>();
builder.Services.AddScoped<IAlunoService, AlunoService>();
builder.Services.AddScoped<IEmprestimoService, EmprestimoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Deep Health Check Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = $"{entry.Value.Duration.TotalMilliseconds:F1}ms"
                }
            )
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
    }
});

app.MapControllers();

app.Run();