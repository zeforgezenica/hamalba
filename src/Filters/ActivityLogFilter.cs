using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace hamalba.Filters
{
    public class ActivityLogFilter : IAsyncActionFilter
    {
        private readonly ILogger<ActivityLogFilter> _logger;

        public ActivityLogFilter(ILogger<ActivityLogFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            string email = "anonymous";

            if (user.Identity?.IsAuthenticated == true)
            {
                email = user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";
            }
            else
            {
                await next();
                return;
            }

            var path = context.HttpContext.Request.Path;
            var method = context.HttpContext.Request.Method;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var shortLog = $"[SHORT] [{timestamp}] Email: {email} | {method} {path}";

            string detailedLog = $"[DETAIL] [{timestamp}] IP: {ipAddress} | Email: {email} | Action: {method} {path}";

            if (path.ToString().ToLower().Contains("edit"))
            {
                var arguments = context.ActionArguments;
                string argsJson = JsonSerializer.Serialize(arguments, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                detailedLog = $"[DETAIL] [{timestamp}] IP: {ipAddress} | Email: {email} | EDIT DETECTED | {method} {path} | Arguments: {argsJson}";
            }

            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(logDir);

            var activityLogPath = Path.Combine(logDir, $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            await File.AppendAllTextAsync(activityLogPath, detailedLog + Environment.NewLine);

            _logger.LogInformation(shortLog);
            _logger.LogInformation(detailedLog);

            await next();
        }
    }
}
