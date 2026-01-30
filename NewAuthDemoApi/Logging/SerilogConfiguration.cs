using Serilog;
using Serilog.Events;


namespace AuthDemoApi.Logging
{
    public static class SerilogConfiguration
    {
        public static void Configure()
        {
             Log.Logger = new LoggerConfiguration()

        // Minimum level
        .MinimumLevel.Information()

        // Ignore framework noise
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft.Data.SqlClient", LogEventLevel.Fatal)
        // Enrich context (CorrelationId, User, etc.)
        .Enrich.FromLogContext()

        // -------- CONSOLE (For Dev) --------
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] " +
            "[Req:{CorrelationId}] " +
            "[Trace:{TraceId}] " +
            "[User:{UserId}] " +
            "[Email:{Username}] " +
            "[IP:{IP}] " +
            "{Message:lj}{NewLine}{Exception}"
        )

        // -------- INFO LOG FILE --------
        .WriteTo.File(
            "Logs/info-.log",
            restrictedToMinimumLevel: LogEventLevel.Information,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] " +
            "[Req:{CorrelationId}] " +
            "[Trace:{TraceId}] " +
            "[User:{UserId}] " +
            "[Email:{Username}] " +
            "[IP:{IP}] " +
            "{Message:lj}{NewLine}{Exception}"
        )

        // -------- ERROR LOG FILE --------
        .WriteTo.File(
            "Logs/error-.log",
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] " +
            "[Req:{CorrelationId}] " +
            "[Trace:{TraceId}] " +
            "[User:{UserId}] " +
            "[Email:{Username}] " +
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