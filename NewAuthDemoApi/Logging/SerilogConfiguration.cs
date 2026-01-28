using Serilog;
using Serilog.Events;
using System.Diagnostics;


namespace AuthDemoApi.Logging
{
    public static class SerilogConfiguration
    {
        public static void Configure()
        {
            Log.Logger=new LoggerConfiguration()
            //Minimum level
            .MinimumLevel.Information()
            //ignore framework noise
            .MinimumLevel.Override("Microsoft",LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
            //Add context info
            .Enrich.FromLogContext()//Without it → CorrelationId won’t appear.
            //output sinks
.WriteTo.Console(outputTemplate:
  "[{Timestamp:HH:mm:ss} {Level:u3}] " +
  "[Req:{CorrelationId}] " +
  "[Trace:{TraceId}] " +
  "[UserId:{UserId}] " +
  "[Username:{Username}] " +
  "[IP:{IP}] " +
  "{Message:lj}{NewLine}{Exception}"
)
        .WriteTo.File("Logs/app-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate:
  "[{Timestamp:HH:mm:ss} {Level:u3}] " +
  "[Req:{CorrelationId}] " +
  "[Trace:{TraceId}] " +
  "[UserId:{UserId}] " +
  "[Username:{Username}] " +
  "[IP:{IP}] " +
  "{Message:lj}{NewLine}{Exception}"
        )
        .CreateLogger();
        }
    
    
    }
}
// SerilogConfiguration.cs  → Setup
// Program.cs              → Call
// Controllers/Services    → Use