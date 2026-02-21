using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bank_Slip_Scanner_App.Data;
using Bank_Slip_Scanner_App.Models;
using Bank_Slip_Scanner_App.Services;
using Microsoft.AspNetCore.Identity.Data;

namespace Bank_Slip_Scanner_App.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class RegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            ILogger<RegisterController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;

        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.NomComplet) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { success = false, message = "tous les champs sont requis." });

                }
                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new { success = false, message = "le mot de passe doit faire au moins 8 caractéres." });
                }
                var email = request.Email.Trim().ToLower();
                bool emailExists = await _context.Users.AnyAsync(u => u.Email == email);
                if (emailExists)
                {
                    return BadRequest(new { success = false, message = "cet email est déjà utilisé." });
                }   // separation nom/prenom
                var parts = request.NomComplet.Trim().Split(' ', 2);
                var prenom = parts.Length > 0 ? parts[0] : request.NomComplet;
                var nom = parts.Length > 1 ? parts[1] : "";
                // hachage
                var (hash, salt) = _passwordHasher.HashPassword(request.Password);
                // creation de l'objet Utilisateur 
                var user = new Users
                {
                    Email = email,
                    MotDePasseHash = hash,
                    Salt = salt,
                    Prenom = prenom,
                    Nom = nom,
                    NomComplet = request.NomComplet,
                    Role = "Agent",
                    Actif = true,
                    DateCreation = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Nouvel utilisateur inscrit : {email}");
                return Ok(new
                {
                    success = true,
                    message = "Compte créé avec succès!",
                    idUsers = user.IdUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur register");
                return StatusCode(500, new { success = false, message = ex.Message, detail = ex.InnerException?.Message });
            }
        }
    }
    public class RegisterRequest
    {
        public string NomComplet { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
