using AuthDemoApi.Data;
using Microsoft.EntityFrameworkCore;
using AuthDemoApi.Helper;
using AuthDemoApi.Extensions;
using System.Text;
using AuthDemoApi.Logging;
using AuthDemoApi.Middleware;
using Microsoft.IdentityModel.Tokens;
using Serilog;

SerilogConfiguration.Configure();
Serilog.Log.Information("Application starting up...");
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddMvc();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") // frontend
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // 🔥 REQUIRED for cookies
    });
});

// 🔹 DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 JWT helper
builder.Services.AddScoped<JwtService>();

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"]!);

// 🔹 JWT auth (NOT global)
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });


// 🔒 Rate Limiter (configured in Extensions/RateLimiterExtensions.cs)
builder.Services.AddCustomRateLimiter();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend"); //If AllowCredentials() is missing → cookies won’t attach.
app.UseAuthentication();
app.UseAuthorization();
//Order matters.
app.UseMiddleware<CorrelationIdMiddleware>();
//app.UseHttpsRedirection();
app.UseMiddleware<UserContextMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    // Don't log 2xx and 3xx as Information
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null || httpContext.Response.StatusCode >= 500)
            return Serilog.Events.LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return Serilog.Events.LogEventLevel.Warning;

        return Serilog.Events.LogEventLevel.Debug;
    };
});//serilog
app.UseRateLimiter();
// Map controller endpoints
app.MapControllers(); // ✅ This is the key line for your AuthController to work


app.Run();


//🔹 OVERVIEW FIRST

//This setup creates a secure login system using JWT +refresh tokens.

//You’ve got:

//✅ User table → stores login + refresh token info

//✅ JwtService → creates access & refresh tokens

//✅ AuthController → handles login + refresh flow

//✅ UserController → protected API (needs JWT)

//✅ Proper CORS + authentication setup in Program.cs

