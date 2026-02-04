using Hangfire.Dashboard;

namespace NovaSaaSWebAPI.Middleware
{
    /// <summary>
    /// HangfireAuthorizationFilter - Chỉ cho phép MasterAdmin truy cập Hangfire Dashboard.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Development: Cho phép tất cả
            if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                return true;
            }

            // Production: Yêu cầu MasterAdmin role
            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return user.IsInRole("MasterAdmin");
        }
    }
}
