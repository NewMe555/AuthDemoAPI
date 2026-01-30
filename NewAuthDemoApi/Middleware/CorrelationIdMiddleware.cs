using System.Diagnostics;
using Serilog.Context;

namespace AuthDemoApi.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string HeaderKey="X-Correlation_Id";
        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next=next;
        }

        public async Task Invoke(HttpContext context)
        {
            //create unique id per request
            var correlationId=Guid.NewGuid().ToString();
            //send it to client(React/Postman can see it)
            context.Response.Headers["X-Correlation-Id"]=correlationId;
           var traceId = Activity.Current?.TraceId.ToString();
           
            //attach to all logs in the request
             using (LogContext.PushProperty("CorrelationId", correlationId))

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
           

        }
        
    }
}
// 1️⃣ Create middleware
// 2️⃣ Register it
// 3️⃣ Run app
// 4️⃣ Hit login API
// 5️⃣ Copy ONE log line here
// 6️⃣ Tell me: “Header visible / Not visible”