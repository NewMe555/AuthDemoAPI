using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace AuthDemoApi.Extensions;

public static class RateLimiterExtensions
{
   public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            //policies define
            //User A (IP: 1.1.1.1) → apne 5 tokens
            // User B (IP: 2.2.2.2) → apne 5 tokens
            //policy 1
           options.AddPolicy("login", context =>
            {
             var email =
             context.Request.HasFormContentType
             ? context.Request.Form["email"].ToString().ToLower()
              : "unknown";

              return RateLimitPartition.GetTokenBucketLimiter(
              partitionKey: $"login:{email}",
              factory: _ => new TokenBucketRateLimiterOptions
              {
            TokenLimit = 5,                      // max attempts
            TokensPerPeriod = 5,                 // full refill
            ReplenishmentPeriod = TimeSpan.FromMinutes(15),
            AutoReplenishment = true,
            QueueLimit = 0
              });
             });

// Why this is SAFE ✅
// Only the attacked account is limited
// Other users unaffected
// Bots can’t brute-force one account
                 
                 //policy 2
            options.AddPolicy("refresh", context =>
             {
    var userId =
        context.User?.FindFirst("sub")?.Value ?? "anonymous";

    return RateLimitPartition.GetTokenBucketLimiter(
        partitionKey: $"refresh:{userId}",
        factory: _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            TokensPerPeriod = 10,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true,
            QueueLimit = 0
        });
});
// Why this works 🔒
// One user → one bucket
// Stolen refresh token can’t be spammed
// IP changes don’t matter
options.OnRejected = async (context, cancellationToken) =>
{
     var response = context.HttpContext.Response;

    // 1️⃣ HTTP standard status code
    response.StatusCode = StatusCodes.Status429TooManyRequests;

    // 2️⃣ Client ko hint dena kab retry kare
    // (seconds – approx, exact hona zaroori nahi)
    response.Headers["Retry-After"] = "60";

    // 3️⃣ Generic message (endpoint leak nahi)
    await response.WriteAsync(
        "Too many requests. Please try again later.",
        cancellationToken);
};
   });      
//Don’t leak endpoint names in headers in prod.
        return services;
    
        }}  


//         /login
//  ├─ Rate limit by email
//  ├─ Optional email+IP
//  ├─ Generic error message
//  ├─ Failure counter
//  └─ Temporary account lock

// /refresh
//  ├─ Rate limit by userId
//  ├─ Refresh token rotation
//  └─ Revoke on abuse