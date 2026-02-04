using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NovaSaaS.WebApi.HealthChecks
{
    /// <summary>
    /// StorageHealthCheck - Kiểm tra dung lượng ổ đĩa cho file uploads.
    /// </summary>
    public class StorageHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StorageHealthCheck> _logger;
        private const long MIN_FREE_SPACE_GB = 5; // Cảnh báo khi còn dưới 5GB

        public StorageHealthCheck(
            IConfiguration configuration,
            ILogger<StorageHealthCheck> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var basePath = _configuration["FileStorage:BasePath"] ?? "uploads/documents";
                var fullPath = Path.GetFullPath(basePath);
                
                // Lấy root drive
                var rootPath = Path.GetPathRoot(fullPath) ?? "C:\\";
                var driveInfo = new DriveInfo(rootPath);

                var freeSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                var totalSpaceGB = driveInfo.TotalSize / (1024.0 * 1024 * 1024);
                var usedPercent = ((totalSpaceGB - freeSpaceGB) / totalSpaceGB) * 100;

                var data = new Dictionary<string, object>
                {
                    { "drive", driveInfo.Name },
                    { "free_space_gb", Math.Round(freeSpaceGB, 2) },
                    { "total_space_gb", Math.Round(totalSpaceGB, 2) },
                    { "used_percent", Math.Round(usedPercent, 2) },
                    { "upload_path", fullPath }
                };

                // Unhealthy nếu còn dưới 1GB
                if (freeSpaceGB < 1)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        $"Critical: Only {freeSpaceGB:F2}GB free space remaining!",
                        data: data));
                }

                // Degraded nếu còn dưới 5GB
                if (freeSpaceGB < MIN_FREE_SPACE_GB)
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"Low disk space: {freeSpaceGB:F2}GB remaining",
                        data: data));
                }

                // Kiểm tra thư mục có thể ghi được không
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                // Test write permission
                var testFile = Path.Combine(fullPath, ".health_check_test");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        $"Cannot write to upload directory: {ex.Message}",
                        data: data));
                }

                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Storage OK: {freeSpaceGB:F2}GB free ({usedPercent:F1}% used)",
                    data: data));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Storage health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Storage check error: {ex.Message}",
                    exception: ex));
            }
        }
    }
}
