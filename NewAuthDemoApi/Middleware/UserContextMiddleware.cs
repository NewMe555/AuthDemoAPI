using System.Security.Claims;
using Serilog.Context;

namespace AuthDemoApi.Middleware
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;
        public UserContextMiddleware(RequestDelegate next)
        {
         _next=next;   
        }

        public async Task Invoke(HttpContext context)
    {
           var user = context.User;

    var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
    var username = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
    var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
             ?? context.Connection.RemoteIpAddress?.ToString()
             ?? "Unknown";

    using (LogContext.PushProperty("UserId", userId))
    using (LogContext.PushProperty("Username", username))
        using (LogContext.PushProperty("IP", ip))
        {
            await _next(context);
        }
    }
    }
}