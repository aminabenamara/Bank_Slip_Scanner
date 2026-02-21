using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bank_Slip_Scanner_App.Data;
using Bank_Slip_Scanner_App.Models;
using System.Security.Cryptography;
namespace Bank_Slip_Scanner_App.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<ForgotPasswordController> logger;
        

        public ForgotPasswordController(ApplicationDbContext context, ILogger<ForgotPasswordController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

       
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "l'email est requis" });

                }
                var email = request.Email.Trim().ToLower();
                // chercher l'utilisateur 
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                // sécurite : si l'utilisateur n'existe pas , on fait semblant que ca a marche 
                if (user == null)
                {
                   _logger.LogWarring("email introvable : {Email}", email);
                    return Ok(new { success = true, message = "si cet email existe, un lien a été envoyé." });
                    // générer le token
                    var token = GenerateResertToken();
                    // mettre à jour l'utilisateur 
                    user.TokenReinitialisation = token;
                    user.DateExpirationToken = DateTime.Now.AddHours(24);
                    user.DateModification = DateTime.Now;
                    // sauvgarder dans sql
                    await _context.SaveChangesAsync();
                    // on crée le lien 
                    var lienReset = $"http://localhost:4200/reset-password?token={token}";
                    Logger.LogWarning("[simulation email] clique ici pour reset : {lien}", lienReset);
                    return Ok(new
                    {
                        success = true,
                        message = "un lien de réinitialisation a été envoyé à votre email"
                    });

                }
            } catch (Exception ex) {
                this._logger.LogError(ex, "Erreur serveur");
                return StatusCode(500, new { success = false, message = "erreur interne." });
            }

        }
        private string GenerateResertToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 64)


        }


        private void Replace(string v1, string v2)
        {
            throw new NotImplementedException();
        }
        public class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }
    }
}