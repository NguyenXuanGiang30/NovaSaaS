using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Interfaces.Caching;
using NovaSaaS.Application.Jobs;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Infrastructure.Caching;
using NovaSaaS.Infrastructure.Configurations;
using NovaSaaS.Infrastructure.Persistence;
using NovaSaaS.Infrastructure.Services;
using NovaSaaS.Infrastructure.Migrations;
using NovaSaaS.WebApi.Configuration;
using NovaSaaS.WebApi.HealthChecks;
using NovaSaaS.Infrastructure.SignalR;
using NovaSaaSWebAPI.Middleware;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using Asp.Versioning;
using NovaSaaS.Application.Interfaces.Inventory;
using NovaSaaS.Application.Interfaces.Business;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CORE SERVICES
// ========================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // Cho TenantMiddleware caching

// ========================================
// 1.5 API VERSIONING
// ========================================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ========================================
// 2. JWT CONFIGURATION
// ========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() 
    ?? new JwtSettings();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // Không cho phép lệch thời gian
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Tự động set Tenant context từ JWT claims
            var tenantService = context.HttpContext.RequestServices.GetRequiredService<ITenantService>();
            
            var tenantIdClaim = context.Principal?.FindFirst("tenant_id")?.Value;
            var schemaNameClaim = context.Principal?.FindFirst("schema_name")?.Value;
            
            if (Guid.TryParse(tenantIdClaim, out var tenantId) && !string.IsNullOrEmpty(schemaNameClaim))
            {
                tenantService.SetTenant(tenantId, schemaNameClaim);
            }
            
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Nếu request có access token trong query string và path bắt đầu bằng /hubs/
            if (!string.IsNullOrEmpty(accessToken) &&
                (context.HttpContext.Request.Path.StartsWithSegments("/hubs")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ========================================
// 3. APPLICATION SERVICES (DI Registration)
// ========================================
// Domain Interfaces
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<ITenantRegistrationService, NovaSaaS.Application.Services.TenantRegistrationService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Inventory Services
builder.Services.AddScoped<NovaSaaS.Application.Services.Inventory.CategoryService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.Inventory.UnitService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.Inventory.ProductService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.Inventory.WarehouseService>();
builder.Services.AddScoped<IStockService, NovaSaaS.Application.Services.Inventory.StockService>();

// Business Services (Sales & CRM)
builder.Services.AddScoped<ICustomerService, NovaSaaS.Application.Services.Business.CustomerService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.Business.OrderService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.Business.InvoiceService>();

// Master Admin Services
builder.Services.AddScoped<NovaSaaS.Application.Services.Master.SubscriptionService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.Master.UsageTrackingService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.Master.SystemLogService>();

// Payment Services
builder.Services.AddScoped<IPaymentService, NovaSaaS.Infrastructure.Services.Payment.StripePaymentService>();

// Email Services
builder.Services.AddScoped<IEmailService, NovaSaaS.Infrastructure.Services.Email.SmtpEmailService>();

// Notification Services
builder.Services.AddScoped<NovaSaaS.Application.Interfaces.INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.NotificationManagementService>();

// Team Management Services
builder.Services.AddScoped<NovaSaaS.Application.Services.Identity.UserManagementService>();

// Analytics Services
builder.Services.AddScoped<NovaSaaS.Application.Services.Analytics.DashboardService>();

// Export Services
builder.Services.AddScoped<NovaSaaS.Application.Services.Export.IExportService, NovaSaaS.Application.Services.Export.ExcelExportService>();

// AI Function Services
builder.Services.AddScoped<NovaSaaS.Application.Interfaces.AI.IAIFunctionService, NovaSaaS.Application.Services.AI.AIFunctionService>();

// ========================================
// RATE LIMITING (Security)
// ========================================
builder.Services.AddTenantRateLimiting();

// ========================================
// OPENTELEMETRY (Observability)
// ========================================
builder.Services.AddNovaSaaSOpenTelemetry(builder.Configuration);

// ========================================
// MIGRATION RUNNER
// ========================================
builder.Services.AddScoped<SchemaMigrationRunner>();

// ========================================
// REDIS CACHING
// ========================================
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
var redisInstanceName = builder.Configuration["Redis:InstanceName"] ?? "NovaSaaS_";

// Register Redis connection multiplexer (singleton)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
    return ConnectionMultiplexer.Connect(configuration);
});

// ========================================
// 4. SIGNALR & REAL-TIME
// ========================================
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString, options => {
        options.Configuration.ChannelPrefix = RedisChannel.Literal("NovaSaaS_SignalR");
    });


// Register distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = redisInstanceName;
});

// Register cache service
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// AI Services (RAG Pipeline)
builder.Services.AddScoped<NovaSaaS.Application.Interfaces.AI.IChunkingService, NovaSaaS.Application.Services.AI.ChunkingService>();
builder.Services.AddScoped<NovaSaaS.Application.Interfaces.AI.IEmbeddingService, NovaSaaS.Infrastructure.AI.GeminiEmbeddingService>();
builder.Services.AddScoped<NovaSaaS.Application.Interfaces.AI.IChatCompletionService, NovaSaaS.Infrastructure.AI.GeminiChatService>();
builder.Services.AddScoped<NovaSaaS.Application.Interfaces.AI.ITextExtractionService, NovaSaaS.Infrastructure.AI.TextExtractionService>();
builder.Services.AddScoped<NovaSaaS.Application.Interfaces.AI.IVectorSearchService, NovaSaaS.Infrastructure.Services.AI.VectorSearchService>();
builder.Services.AddScoped<NovaSaaS.Application.Services.AI.RAGService>();

// ========================================
// 4. DATABASE CONFIGURATION
// ========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.UseVector()); // Enable pgvector
    
    // Quan trọng: Cache model riêng cho mỗi schema (Multi-tenancy)
    options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, 
                          TenantModelCacheKeyFactory>();
});

// ========================================
// 5. API DOCUMENTATION
// ========================================
builder.Services.AddOpenApi();

// ========================================
// 6. HANGFIRE BACKGROUND JOBS
// ========================================
var hangfireSchema = builder.Configuration["Hangfire:Schema"] ?? "hangfire";
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options
        .UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")),
        new PostgreSqlStorageOptions
        {
            SchemaName = hangfireSchema,
            PrepareSchemaIfNecessary = true
        }));

builder.Services.AddHangfireServer();

// Register background jobs
builder.Services.AddScoped<SubscriptionCheckJob>();
builder.Services.AddScoped<DocumentProcessingJob>();
builder.Services.AddScoped<InvoiceReminderJob>();

// ========================================
// 7. HEALTH CHECKS
// ========================================
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" })
    .AddRedis(
        redisConnectionString,
        name: "redis",
        tags: new[] { "cache", "redis" })
    .AddCheck<GeminiHealthCheck>(
        "gemini-ai",
        tags: new[] { "ai", "external" })
    .AddCheck<StorageHealthCheck>(
        "storage",
        tags: new[] { "storage", "disk" });

var app = builder.Build();

// ========================================
// HTTP REQUEST PIPELINE
// ========================================

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Giao diện API tại /scalar/v1
}

app.UseHttpsRedirection();

// ========================================
// GLOBAL EXCEPTION MIDDLEWARE - "Bộ ghi chép lỗi trung tâm"
// Đặt trước Tenant Middleware để bắt tất cả exceptions
// ========================================
app.UseMiddleware<NovaSaaS.WebApi.Middleware.GlobalExceptionMiddleware>();

// ========================================
// TENANT MIDDLEWARE - "Bộ điều hướng trung tâm"
// Đứng trước Authentication để thiết lập context cho anonymous requests
// ========================================
app.UseTenantMiddleware();

// ========================================
// AUTHENTICATION & AUTHORIZATION
// ========================================
app.UseAuthentication();
app.UseAuthorization();

// ========================================
// RATE LIMITING (after auth to have tenant context)
// ========================================
app.UseRateLimiter();

app.MapControllers();

// ========================================
// SIGNALR HUB
// ========================================
app.MapHub<NotificationHub>("/hubs/notifications");

// ========================================
// HEALTH CHECK ENDPOINTS
// ========================================
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data,
                exception = e.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
});

// Quick health check (chỉ PostgreSQL - cho load balancer)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

// Ready check (tất cả services)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true
});
// ========================================
// HANGFIRE DASHBOARD
// ========================================
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "NovaSaaS Background Jobs"
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<SubscriptionCheckJob>(
    "subscription-check",
    job => job.ExecuteAsync(),
    Cron.Daily(0, 0)); // Run daily at 00:00 UTC

RecurringJob.AddOrUpdate<InvoiceReminderJob>(
    "invoice-reminder",
    job => job.ExecuteAsync(),
    Cron.Daily(9, 0)); // Run daily at 09:00 UTC (16:00 VN)

// ========================================
// STARTUP LOGGING
// ========================================
app.Logger.LogInformation("🚀 NovaSaaS API Started");
app.Logger.LogInformation("📊 Environment: {Env}", app.Environment.EnvironmentName);
app.Logger.LogInformation("🔐 JWT Authentication: Enabled");
app.Logger.LogInformation("🌐 URLs: {Urls}", string.Join(", ", app.Urls));

app.Run();

public partial class Program { }
