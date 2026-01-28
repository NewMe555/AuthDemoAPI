using AuthDemoApi.Data;
using AuthDemoApi.Data.DTOs;
using AuthDemoApi.Helper;
using AuthDemoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthDemoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }      

[HttpPost("login")]
[EnableRateLimiting("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    // 1️⃣ Find user
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.username);

    if (user == null || !BCrypt.Net.BCrypt.Verify(dto.password, user.PasswordHash))
        return Unauthorized("Invalid credentials");

    // 2️⃣ Revoke ALL existing ACTIVE refresh tokens (single-session rule)
    var activeTokens = await _db.RefreshTokens
        .Where(rt =>
            rt.UserId == user.Id &&
            !rt.IsRevoked).ExecuteUpdateAsync(setters=>setters.SetProperty(rt=>rt.IsRevoked,true));
        


    // 3️⃣ Generate tokens
    var accessToken = _jwt.GenerateAccessToken(user);
    var refreshToken = _jwt.GenerateRefreshToken();

    var refreshExpiry = DateTime.UtcNow.AddDays(7);

    // 4️⃣ Hash refresh token
    var refreshTokenHash = TokenHasher.Hash(refreshToken);

    // 5️⃣ Save NEW refresh token
    _db.RefreshTokens.Add(new RefreshToken
    {
        UserId = user.Id,
        TokenHash = refreshTokenHash,
        ExpiresAt = refreshExpiry,
        IsRevoked = false
    });

    user.LastLoginAt = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    // 6️⃣ Set refresh token cookie
    Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = false, // true in prod
        SameSite = SameSiteMode.Lax,
        Expires = refreshExpiry,
         Path = "/"
    });

    // 7️⃣ Return access token
    return Ok(new { accessToken });
}

 






        //🔹 2️⃣ Refresh Token Endpoint
        //Used when:
            //Access token expired.
            //But refresh token(in cookie) is still valid.
        [HttpPost("refresh")]
        [EnableRateLimiting("refresh")]
public async Task<IActionResult> Refresh()
{
    // 1️⃣ Read refresh token from HttpOnly cookie
    var refreshToken = Request.Cookies["refreshToken"];
    if (string.IsNullOrEmpty(refreshToken))
        return Unauthorized("Missing refresh token");

    var refreshTokenHash = TokenHasher.Hash(refreshToken);

    // 2️⃣ Find token in DB
    var storedToken = await _db.RefreshTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt =>
            rt.TokenHash == refreshTokenHash &&
            !rt.IsRevoked &&
            rt.ExpiresAt > DateTime.UtcNow);
 
    if (storedToken == null)
        return Unauthorized("Invalid or expired refresh token");

    var user = storedToken.User;

    // 3️⃣ ROTATION: revoke old refresh token
    storedToken.IsRevoked = true;

    // 4️⃣ Generate NEW refresh token
    var newRefreshToken = _jwt.GenerateRefreshToken();
    var newRefreshTokenHash = TokenHasher.Hash(newRefreshToken);

    var newRefreshTokenEntity = new RefreshToken
    {
        UserId = user.Id,
        TokenHash = newRefreshTokenHash,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    };

    _db.RefreshTokens.Add(newRefreshTokenEntity);

    // 5️⃣ Generate NEW access token
    var newAccessToken = _jwt.GenerateAccessToken(user);

    await _db.SaveChangesAsync();

    // 6️⃣ Update cookie (overwrite old one)
    Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = false, // true in prod
        SameSite = SameSiteMode.Lax,
        Expires = newRefreshTokenEntity.ExpiresAt,
    Path = "/"
    });

    // 7️⃣ Return new JWT
    return Ok(new { accessToken = newAccessToken });
}



//       ✅ Summary
//       ✅ Key Improvements

//No null assignment → avoids DbUpdateException.

//Checks user existence → avoids crashes if user doesn’t exist.

//Graceful error handling → returns meaningful message if DB fails.

//Optional token expiry update → ensures refresh token is not valid anymore.
       
        [HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var refreshToken = Request.Cookies["refreshToken"];

    if (!string.IsNullOrEmpty(refreshToken))
    {
        var tokenHash = TokenHasher.Hash(refreshToken);

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && !rt.IsRevoked);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    Response.Cookies.Delete("refreshToken");
    return Ok(new { message = "Logged out" });
}


    }

    
}