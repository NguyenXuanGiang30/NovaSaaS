using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Infrastructure.Persistence;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Application.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <summary>
    /// Schema Migration Runner - Thực thi migrations cho tất cả tenant schemas.
    /// Hỗ trợ parallel processing với safety mechanisms.
    /// </summary>
    public class SchemaMigrationRunner
    {
        private readonly ILogger<SchemaMigrationRunner> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _connectionString;

        // Configuration
        private const int DefaultConcurrency = 10;
        private const int MaxRetries = 3;

        public SchemaMigrationRunner(
            ILogger<SchemaMigrationRunner> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not configured");
        }

        /// <summary>
        /// Chạy migrations cho tất cả tenant schemas.
        /// </summary>
        public async Task<MigrationResult> RunMigrationsAsync(
            int concurrency = DefaultConcurrency,
            CancellationToken cancellationToken = default)
        {
            var result = new MigrationResult();
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("🚀 Starting schema migrations with concurrency: {Concurrency}", concurrency);

            try
            {
                // Step 1: Lấy danh sách tất cả tenants từ public schema
                var tenants = await GetActiveTenantsAsync(cancellationToken);
                result.TotalTenants = tenants.Count;

                _logger.LogInformation("📋 Found {Count} active tenants to migrate", tenants.Count);

                // Step 2: Chạy migrations song song
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = concurrency,
                    CancellationToken = cancellationToken
                };

                var errors = new ConcurrentBag<TenantMigrationError>();
                var successCount = 0;

                await Parallel.ForEachAsync(tenants, parallelOptions, async (tenant, token) =>
                {
                    var tenantResult = await MigrateTenantSchemaAsync(tenant, token);
                    
                    if (tenantResult.Success)
                    {
                        Interlocked.Increment(ref successCount);
                        _logger.LogInformation("✅ Migrated tenant: {Subdomain} ({SchemaName})", 
                            tenant.Subdomain, tenant.SchemaName);
                    }
                    else
                    {
                        errors.Add(new TenantMigrationError
                        {
                            TenantId = tenant.Id,
                            Subdomain = tenant.Subdomain,
                            SchemaName = tenant.SchemaName,
                            ErrorMessage = tenantResult.ErrorMessage,
                            Exception = tenantResult.Exception
                        });

                        _logger.LogError("❌ Failed to migrate tenant: {Subdomain} - {Error}",
                            tenant.Subdomain, tenantResult.ErrorMessage);
                    }
                });

                result.SuccessCount = successCount;
                result.FailedCount = errors.Count;
                result.Errors = errors.ToList();
                result.Success = errors.Count == 0;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.ErrorMessage = "Migration was cancelled";
                _logger.LogWarning("⚠️ Migration cancelled by user");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                _logger.LogError(ex, "❌ Migration runner failed with exception");
            }

            result.Duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("📊 Migration completed in {Duration}. Success: {Success}, Failed: {Failed}",
                result.Duration, result.SuccessCount, result.FailedCount);

            return result;
        }

        /// <summary>
        /// Lấy danh sách tenants đang hoạt động từ public.Tenants.
        /// </summary>
        private async Task<List<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken)
        {
            // Sử dụng raw Npgsql connection để query public.Tenants
            var tenants = new List<Tenant>();
            
            await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            await using var command = new Npgsql.NpgsqlCommand(
                @"SELECT ""Id"", ""Subdomain"", ""SchemaName"", ""Name"", ""Status"" 
                  FROM public.""Tenants"" 
                  WHERE ""Status"" = 1 
                  ORDER BY ""Subdomain""", 
                connection);
            
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            while (await reader.ReadAsync(cancellationToken))
            {
                tenants.Add(new Tenant
                {
                    Id = reader.GetGuid(0),
                    Subdomain = reader.GetString(1),
                    SchemaName = reader.GetString(2),
                    Name = reader.GetString(3),
                    Status = TenantStatus.Active
                });
            }
            
            return tenants;
        }

        /// <summary>
        /// Migrate một tenant schema cụ thể.
        /// </summary>
        private async Task<TenantMigrationResult> MigrateTenantSchemaAsync(
            Tenant tenant,
            CancellationToken cancellationToken)
        {
            var result = new TenantMigrationResult { TenantId = tenant.Id };
            var attempts = 0;

            while (attempts < MaxRetries)
            {
                attempts++;
                try
                {
                    // Build connection string cho tenant schema
                    var tenantConnectionString = BuildTenantConnectionString(tenant.SchemaName);
                    
                    // Sử dụng DbContextFactory để tạo context với tenant-specific connection
                    using var scope = _serviceProvider.CreateScope();
                    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
                    var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                    
                    // Set tenant context
                    tenantService.SetTenant(tenant.Id, tenant.SchemaName);
                    
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                    optionsBuilder.UseNpgsql(tenantConnectionString);

                    using var context = new ApplicationDbContext(
                        optionsBuilder.Options, 
                        tenantService, 
                        currentUserService);

                    // Kiểm tra pending migrations
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                    var pendingList = pendingMigrations.ToList();

                    if (pendingList.Count == 0)
                    {
                        _logger.LogDebug("No pending migrations for tenant: {Subdomain}", tenant.Subdomain);
                    }
                    else
                    {
                        _logger.LogInformation("Applying {Count} migrations to tenant: {Subdomain}",
                            pendingList.Count, tenant.Subdomain);

                        // Thực thi migrations
                        await context.Database.MigrateAsync(cancellationToken);
                    }

                    result.Success = true;
                    result.MigrationsApplied = pendingList.Count;
                    return result;
                }
                catch (Exception ex) when (attempts < MaxRetries)
                {
                    _logger.LogWarning(ex, "Retry {Attempt}/{MaxRetries} for tenant: {Subdomain}",
                        attempts, MaxRetries, tenant.Subdomain);
                    
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)), cancellationToken);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    result.Exception = ex;
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Build connection string với search_path cho tenant schema.
        /// </summary>
        private string BuildTenantConnectionString(string schemaName)
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(_connectionString)
            {
                SearchPath = schemaName
            };
            return builder.ConnectionString;
        }

        /// <summary>
        /// Chạy migration cho một tenant cụ thể (single tenant).
        /// </summary>
        public async Task<TenantMigrationResult> MigrateSingleTenantAsync(
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            var tenant = await GetTenantByIdAsync(tenantId, cancellationToken);
            
            if (tenant == null)
            {
                return new TenantMigrationResult
                {
                    TenantId = tenantId,
                    Success = false,
                    ErrorMessage = "Tenant not found"
                };
            }

            return await MigrateTenantSchemaAsync(tenant, cancellationToken);
        }

        private async Task<Tenant?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            await using var command = new Npgsql.NpgsqlCommand(
                @"SELECT ""Id"", ""Subdomain"", ""SchemaName"", ""Name"", ""Status"" 
                  FROM public.""Tenants"" 
                  WHERE ""Id"" = @Id", 
                connection);
            command.Parameters.AddWithValue("@Id", tenantId);
            
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            if (await reader.ReadAsync(cancellationToken))
            {
                return new Tenant
                {
                    Id = reader.GetGuid(0),
                    Subdomain = reader.GetString(1),
                    SchemaName = reader.GetString(2),
                    Name = reader.GetString(3),
                    Status = (TenantStatus)reader.GetInt32(4)
                };
            }
            
            return null;
        }
    }

    #region Result Classes

    public class MigrationResult
    {
        public bool Success { get; set; }
        public int TotalTenants { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public TimeSpan Duration { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
        public List<TenantMigrationError> Errors { get; set; } = new();
    }

    public class TenantMigrationResult
    {
        public Guid TenantId { get; set; }
        public bool Success { get; set; }
        public int MigrationsApplied { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }

    public class TenantMigrationError
    {
        public Guid TenantId { get; set; }
        public string Subdomain { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }

    #endregion
}
