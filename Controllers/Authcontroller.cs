using Bank_Slip_Scanner_App.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Bank_Slip_Scanner_App.Services;


namespace Bank_Slip_Scanner_App.Controllers
{
    public class AuthController
    {
        [ApiController]
        [Route("api/[controller]")]
        public class IAuthcontroller : ControllerBase
        {
            private readonly IAuthService auth;
            private object _auth;
            private IEnumerable<string> roles;
            private object nomComplet;

            public IAuthcontroller(IAuthService auth)
            {
                _auth = auth;
            }

            [HttpPost("login")]
            [AllowAnonymous]
            public async Task<IActionResult> Login([FromBody] LoginRequest req)
            {
                if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                    return BadRequest(new { sucess = false, message = "Email and password required" });
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var ua = HttpContext.Request.Headers["User-agent"].ToString();

                var result = await _auth.LoginAsync(req.Email, req.Password, req.KeepSignedIn, ip, ua);
                if (result.Success)
                    return Ok(result);
                else
                    Unauthorized(result);
            } 
            [HttpPost("logout")]
            [Authorize]
            public async Task<IActionResult> Logout()
            {
                var idUsers = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _auth.LogoutAsync(idUsers);
                return Ok(new { success = true, message = "logged out" });
            }
            [HttpGet("me")]
            [Authorize]
            public IActionResult Me() => Ok(new
            {
                id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst(ClaimTypes.Email)?.Value,
                nomComplet = User.FindFirst(ClaimTypes.Name)?.Value,
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            });
            [HttpPost("change-password")]
            [Authorize]
            public async Task<IActionResult> ChangePassword([FromBody] changePasswordRequest req)
            {
                var idUsers = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var success = await _auth.ChangePasswordAsync(idUsers, req.OldPassword, req.NewPassword);
                return success
                    ? Ok(new { success = true, message = "password change"
                    })
                    : BadRequest(new { success = false, message = "worng old password"
                    });
            }
        }
    }
}
