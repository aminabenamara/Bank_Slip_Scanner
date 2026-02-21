using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bank_Slip_Scanner_App.Models.DTOs;
using Bank_Slip_Scanner_App.Services;


namespace Bank_Slip_Scanner_App.Controllers
{
   
    
        [ApiController]
        [Route("api/[controller]")]
        public class Authcontroller : ControllerBase
        {
            private readonly IAuthService auth;
        private IAuthService _auth;

        public Authcontroller(IAuthService auth)
            {
            _auth = auth;
            }

            [HttpPost("login")]
            [AllowAnonymous]
            public async Task<IActionResult> Login([FromBody] LoginRequest req)
            {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { sucess = false, message = "Email and password required" });
            }
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var ua = HttpContext.Request.Headers["User-agent"].ToString();

                var result = await _auth.LoginAsync(req.Email, req.Password, req.KeepSignedIn, ip, ua);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
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
            public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
            {
                var idUsers = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var success = await _auth.ChangePasswordAsync(idUsers, req.OldPassword, req.NewPassword);

                if (success)
                    return Ok(new { success = true, message = "password change" });
                else
                    return BadRequest(new { success = false, message = "worng old password" });
            }
        }

    internal class _auth
    {
        internal static async Task<bool> ChangePasswordAsync(int idUsers, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        internal static async Task LoginAsync(string email, string password, bool keepSignedIn, string ip, string ua)
        {
            throw new NotImplementedException();
        }

       
}
}
