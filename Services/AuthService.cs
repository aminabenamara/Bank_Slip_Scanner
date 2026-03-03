using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bank_Slip_Scanner_App.Data;
using Bank_Slip_Scanner_App.Models;
using Bank_Slip_Scanner_App.Models.DTOs;

namespace Bank_Slip_Scanner_App.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(string nomComplet, string email, string password);
        Task<LoginResponse> LoginAsync(string email, string password, bool keepSignedIn, string ip, string userAgent);
        Task<bool> LogoutAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        
        //  REGISTER
       

        public async Task<RegisterResponse> RegisterAsync(string NomComplet, string email, string password)
        {
            try
            {
                email = email.Trim().ToLower();

                // Vérifier si email existe déjà
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (existingUser != null)
                {
                    _logger.LogWarning("Tentative inscription avec email existant: {Email}", email);
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Cet email est déjà utilisé"
                    };
                }

                // Séparer nom et prénom
                var parts = NomComplet.Trim().Split(' ', 2);
                var prenom = parts.Length > 0 ? parts[0] : NomComplet;
                var nom = parts.Length > 1 ? parts[1] : "";

                // Hash password
                var (hash, salt) = _passwordHasher.HashPassword(password);

                // Créer utilisateur
                var user = new Users
                {
                    Email = email,
                    MotDePasseHash = hash,
                    Salt = salt,
                    Prenom = prenom,
                    Nom = nom,
                    NomComplet = NomComplet,
                    Actif = true,
                    EmailVerifie = false,
                    DateCreation = DateTime.Now,
                    DateModification = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nouveau compte créé: {Email} (ID: {UserId})", email, user.IdUsers);

                return new RegisterResponse
                {
                    Success = true,
                    Message = "Compte créé avec succès",
                    UserId = user.IdUsers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription: {Email}", email);
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Erreur serveur. Réessayez plus tard."
                };
            }
        }

        
        //  LOGIN

        public async Task<LoginResponse> LoginAsync(string email, string password, bool keepSignedIn, string ip, string userAgent)
        {
            try
            {
                email = email.Trim().ToLower();

                // Trouver utilisateur
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning("Tentative login email inexistant: {Email}", email);
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Email ou mot de passe incorrect"
                    };
                }

                // Vérifier compte actif
                if (!user.Actif)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Compte désactivé. Contactez l'administrateur."
                    };
                }

                // Vérifier compte verrouillé
                if (user.CompteVerrouille)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Compte verrouillé après plusieurs tentatives échouées."
                    };
                }

                // Vérifier mot de passe
                if (!_passwordHasher.Verify(password, user.MotDePasseHash))
                {
                    user.TentativesConnexion++;

                    if (user.TentativesConnexion >= 5)
                    {
                        user.CompteVerrouille = true;
                        user.DateVerrouillage = DateTime.Now;
                        _logger.LogWarning("Compte verrouillé: {Email}", email);
                    }

                    await _context.SaveChangesAsync();

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Email ou mot de passe incorrect"
                    };
                }

                // Reset tentatives + MAJ dernière connexion
                user.TentativesConnexionEchouees = 0;
                user.DerniereConnexion = DateTime.Now;

                // Générer tokens
                var roles = new[] { "USER" };
                var accessToken = _jwtTokenService.GenerateAccessToken(
                    user.IdUsers,
                    user.Email,
                    user.NomComplet,
                    roles
                );
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // Créer session
                var session = new Session
                {
                    IdUsers = user.IdUsers,
                    TokenSession = accessToken,
                    RefreshToken = refreshToken,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    DateExpiration = DateTime.Now.AddDays(keepSignedIn ? 30 : 1),
                    Actif = true
                };

                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Login réussi: {Email} | IP: {IP}", email, ip);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Connexion réussie",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        Id = user.IdUsers,
                        Email = user.Email,
                        NomComplet = user.NomComplet,
                        Roles = roles
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur login: {Email}", email);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Erreur serveur."
                };
            }
        }

   
        //  LOGOUT
        
        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                var sessions = await _context.Sessions
                    .Where(s => s.IdUsers == userId && s.Actif)
                    .ToListAsync();

                foreach (var session in sessions)
                {
                    session.Actif = false;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Logout réussi: UserID={UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur logout: UserID={UserId}", userId);
                return false;
            }
        }

        
        //  CHANGE PASSWORD
       
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null || !_passwordHasher.Verify(oldPassword, user.MotDePasseHash))
                {
                    return false;
                }

                var (hash, salt) = _passwordHasher.HashPassword(newPassword);
                user.MotDePasseHash = hash;
                user.Salt = salt;
                user.DateModification = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Mot de passe changé: UserID={UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur changement mot de passe: UserID={UserId}", userId);
                return false;
            }
        }
    }
}