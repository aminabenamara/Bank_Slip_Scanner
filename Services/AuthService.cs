using Microsoft.EntityFrameworkCore;
using Bank_Slip_Scanner_App.Data;
using Bank_Slip_Scanner_App.Models;
using Bank_Slip_Scanner_App.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace Bank_Slip_Scanner_App.Services
{
    public interface IAuthService
    {
        Task<LoginRequest> LoginAsync(string email, string password, bool keepSignedIn, string ip, string ua);
        Task<bool> LoginAsync(int idUsers);
        Task<bool> ChangePasswordAsync(int idUsers, string oldPwd, string newPwd);
        Task LogoutAsync(int idUsers);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenService _jwt;
        private readonly IJwtTokenService jwt;
        private readonly ILogger<AuthService> _log;
    

    public AuthService(ApplicationDbContext db, IPasswordHasher hasher, IJwtTokenService jwt, ILogger<AuthService> log)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
            _log = log;

        }
        public async Task<LoginResponse> LoginAsync(
            string email, string passowrd, bool keepSignedIn, string ip, string ua)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
                if (user == null)
                    return Fail("invalid email or password"); // sécurite : ne pas dire " Email introvable"
                // compte actif?
                if (!user.Actif)
                    return Fail("Account disabled.Contact administrator.");
                // compte verrouillé?

                if (user.CompteVerrouille)
                    return Fail("Account locked after 5 attemps. Contact administrator.");
                // verifier mot de passe (BCrypt)
                if (!_hasher.VerifyPassword(passowrd, user.MotDePasseHash))
                {
                   user.TentativesConnexion++;
                    //verrouiller aprés 5 Tentatives
                    if (user.TentativesConnexion >=5)
                    {
                   
                        user.CompteVerrouille = true;
                        user.DateVerrouillage = DateTime.Now;
                        _log.LogWarning("Compte verrouillé: {Email}", email);
                        await _db.SaveChangesAsync();
                        return Fail("Account has been locked");
                    }
                    await _db.SaveChangesAsync();
                    return Fail("Invalid email or password");
                }
                // Rest tentatives + maj derniére connexion 
                user.TentativesConnexion = 0;
                user.DerniereConnexion = DateTime.Now;
                //Rôles (par défaut user si table vide)
                var roles = new[] { "USER" };
                // générer JWT + Refresh Token
                var accessToken = _jwt.GenerateAccessToken(user.IdUsers, user.Email, user.NomComplet, roles);
                var refreshToken = _jwt.GenerateRefreshToken();
                // créer session en DB
                _db.Sessions.Add(new Session
                {
                    IdUsers = user.IdUsers,
                    TokenSession = accessToken,
                    RefreshToken = refreshToken,
                    IpAddress = ip,
                    UserAgent = ua,
                    DateExpiration = DateTime.Now.AddDays(keepSignedIn ? 30 : 1),
                    Actif = true
                });
                await _db.SaveChangesAsync();
                _log.LogInformation("Login OK: {Email} | IP: {IP}", email, ip);
                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        Id = user.IdUsers,
                        Email = user.Email,
                        NomComplet = user.NomComplet,
                        Roles = roles,
                    }
                };
            } catch (Exception ex)
            {
                _log.LogError(ex, "Erreur login: {Email}", email);
                return Fail("Server Error . try again.");


            }
        }
        //LOGOUT
        public async Task<bool> LogoutAsync(int idUsers)
        {
            var sessions = await _db.Sessions
                .Where(s => s.IdUsers == idUsers && s.Actif)
                .ToListAsync();
            sessions.ForEach(s => s.Actif = false);
            await _db.SaveChangesAsync();
            return true;

        }
        // change Password
        public async Task<bool> ChangePasswordAsync(int idUsers, string OldPassword, string NewPassword) {
            var user = await _db.Users.FindAsync(idUsers);
            if (user == null || !_hasher.verify(OldPassword, user.MotDePasseHash))
                return false;
            var (hash, salt) = _hasher.HashPassword(NewPassword);
            user.MotDePasseHash = hash;
            user.Salt = salt;

            await _db.SaveChangesAsync();
            return true;
        }
        private static LoginResponse Fail(String msg) => new() { Success = false, Message = msg };

        Task<LoginRequest> IAuthService.LoginAsync(string email, string password, bool keepSignedIn, string ip, string ua)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LoginAsync(int idUsers)
        {
            throw new NotImplementedException();
        }

        Task IAuthService.LogoutAsync(int idUsers)
        {
            return LogoutAsync(idUsers);
        }
    }
}
