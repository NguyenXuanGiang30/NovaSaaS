using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NovaSaaS.Application.Services.Master;
using NovaSaaS.Domain.Enums;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace NovaSaaS.WebApi.Middleware
{
    /// <summary>
    /// GlobalExceptionMiddleware - Catch và log tất cả unhandled exceptions.
    /// Lưu vào public.SystemLogs để Master Admin theo dõi sức khỏe hệ thống.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, SystemLogService logService)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
                stopwatch.Stop();

                // Log slow requests (> 5 seconds)
                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    await LogSlowRequestAsync(context, logService, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex, "❌ Unhandled exception: {Path}", context.Request.Path);
                
                // Log to SystemLogs
                await LogExceptionAsync(context, logService, ex, stopwatch.ElapsedMilliseconds);
                
                // Return standardized error response
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task LogExceptionAsync(
            HttpContext context, 
            SystemLogService logService, 
            Exception ex,
            long durationMs)
        {
            try
            {
                var tenantId = context.Items.TryGetValue("TenantId", out var tid) && tid is Guid guidId
                    ? guidId
                    : (Guid?)null;

                var clientIp = context.Connection.RemoteIpAddress?.ToString();
                var userId = context.User?.FindFirst("sub")?.Value ?? 
                             context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await logService.LogAsync(
                    tenantId: tenantId,
                    level: DetermineLogLevel(ex),
                    source: $"{context.Request.Method} {context.Request.Path}",
                    message: ex.Message,
                    exception: ex,
                    requestPath: context.Request.Path,
                    httpMethod: context.Request.Method,
                    clientIp: clientIp,
                    durationMs: durationMs,
                    additionalData: GetAdditionalData(context, ex)
                );
            }
            catch (Exception logEx)
            {
                // Don't fail the request if logging fails
                _logger.LogError(logEx, "Failed to log exception to SystemLogs");
            }
        }

        private async Task LogSlowRequestAsync(
            HttpContext context,
            SystemLogService logService,
            long durationMs)
        {
            try
            {
                var tenantId = context.Items.TryGetValue("TenantId", out var tid) && tid is Guid guidId
                    ? guidId
                    : (Guid?)null;

                await logService.LogAsync(
                    tenantId: tenantId,
                    level: SystemLogLevel.Warning,
                    source: $"{context.Request.Method} {context.Request.Path}",
                    message: $"Slow request detected: {durationMs}ms",
                    requestPath: context.Request.Path,
                    httpMethod: context.Request.Method,
                    durationMs: durationMs
                );
            }
            catch
            {
                // Ignore logging failures
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            
            var (statusCode, errorCode, message) = exception switch
            {
                // Validation & Bad Request (more specific first)
                ArgumentNullException => (StatusCodes.Status400BadRequest, Constants.ErrorCodes.VALIDATION_REQUIRED_FIELD, "Thiếu thông tin bắt buộc."),
                FormatException => (StatusCodes.Status400BadRequest, Constants.ErrorCodes.VALIDATION_INVALID_FORMAT, "Định dạng dữ liệu không hợp lệ."),
                ArgumentException argEx => (StatusCodes.Status400BadRequest, Constants.ErrorCodes.VALIDATION_FAILED, argEx.Message),
                
                // Authentication & Authorization
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, Constants.ErrorCodes.AUTH_UNAUTHORIZED, "Không có quyền truy cập."),
                
                // Not Found
                KeyNotFoundException => (StatusCodes.Status404NotFound, Constants.ErrorCodes.RESOURCE_NOT_FOUND, "Không tìm thấy tài nguyên."),
                FileNotFoundException => (StatusCodes.Status404NotFound, Constants.ErrorCodes.RESOURCE_NOT_FOUND, "Không tìm thấy file."),
                
                // Conflict
                InvalidOperationException invEx => (StatusCodes.Status409Conflict, Constants.ErrorCodes.RESOURCE_CONFLICT, invEx.Message),
                
                // Timeout
                TimeoutException => (StatusCodes.Status504GatewayTimeout, Constants.ErrorCodes.SYSTEM_TIMEOUT, "Yêu cầu quá thời gian cho phép."),
                OperationCanceledException => (StatusCodes.Status504GatewayTimeout, Constants.ErrorCodes.SYSTEM_TIMEOUT, "Yêu cầu đã bị hủy."),
                
                // Database errors (hide sensitive info)
                Npgsql.NpgsqlException => (StatusCodes.Status500InternalServerError, Constants.ErrorCodes.SYSTEM_DATABASE_ERROR, "Lỗi kết nối cơ sở dữ liệu."),
                
                // Default
                _ => (StatusCodes.Status500InternalServerError, Constants.ErrorCodes.SYSTEM_UNKNOWN, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.")
            };

            context.Response.StatusCode = statusCode;

            var errorResponse = new Models.ApiErrorResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Details = isDevelopment ? $"{exception.GetType().Name}: {exception.Message}" : null
            };

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private static SystemLogLevel DetermineLogLevel(Exception ex)
        {
            return ex switch
            {
                ArgumentException => SystemLogLevel.Warning,
                UnauthorizedAccessException => SystemLogLevel.Warning,
                KeyNotFoundException => SystemLogLevel.Info,
                InvalidOperationException => SystemLogLevel.Warning,
                OperationCanceledException => SystemLogLevel.Info,
                _ => SystemLogLevel.Error
            };
        }

        private static string? GetAdditionalData(HttpContext context, Exception ex)
        {
            try
            {
                var data = new
                {
                    QueryString = context.Request.QueryString.Value,
                    Headers = new
                    {
                        UserAgent = context.Request.Headers.UserAgent.ToString(),
                        ContentType = context.Request.ContentType,
                        ContentLength = context.Request.ContentLength
                    },
                    ExceptionType = ex.GetType().FullName,
                    InnerExceptionType = ex.InnerException?.GetType().FullName,
                    InnerMessage = ex.InnerException?.Message
                };

                return JsonSerializer.Serialize(data);
            }
            catch
            {
                return null;
            }
        }
    }
}
