using System.Net;
using System.Text.Json;

namespace AuthDemoApi.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger
        )
        {
            _next=next;
            _logger=logger;
        }
    
    public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Unhandled exception caught in middleware Path:{Path}",
                context.Request.Path);
                
    // 🔥 If DB is down → return 503
    if (ex is Microsoft.Data.SqlClient.SqlException)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        await context.Response.WriteAsJsonAsync(new
        {
            message = "Service temporarily unavailable. Please try again later."
        });

        return;
    }

    // 🔹 For all other errors → 500
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

    await context.Response.WriteAsJsonAsync(new
    {
        message = "Something went wrong."
    });
            }
        }

        private async Task HandleException(HttpContext context, Exception ex)
        {
            //log full error with stack trace
            _logger.LogError(ex,
            "Unhandled exception occured. Path:{Path}",
            context.Request.Path);

            //prepare clean response
            context.Response.ContentType="applicatioj/json";
            context.Response.StatusCode=(int)HttpStatusCode.InternalServerError;

            var response = new
            {
                status=500,
                message="Something went wrong.Please try again later",
                traceId=context.TraceIdentifier
            };

            //send to client
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
                );
        }
}}