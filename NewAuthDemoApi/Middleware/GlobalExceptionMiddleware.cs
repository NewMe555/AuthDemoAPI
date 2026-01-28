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
                await HandleException(context,ex);
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