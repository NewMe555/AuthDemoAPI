using AuthDemoApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthDemoApi.Controllers
{
    //Why:
    //[Authorize] → only requests with valid access token can call this API.
    //Returns user info for dashboard.

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController:ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet("profile")]

        public async Task<IActionResult> GetProfile()
        {
            var username = User.Identity?.Name;
            var user=await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized();
            return Ok(new
            {
                user.Id,
                user.Username,
                Avatar = $"https://i.pravatar.cc/150?u={user.Id}",
                user.LastLoginAt
            });
        }


#if DEBUG
/// <summary>
/// testing purpose for global testing
/// ----A fault-injection endpoint.----
/// </summary>
/// <returns></returns>
/// <exception cref="Exception"></exception>
        [HttpGet("crash")]
    public IActionResult Crash()
    {   
        throw new Exception("Test crash");
    }
    #endif

}


}
