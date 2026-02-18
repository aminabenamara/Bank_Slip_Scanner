using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Bank_Slip_Scanner_App.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(int idUsers, string email, string nom_complet, string[] roles);
        string GenerateRefreshToken();
        string GenreateRefreshToken();
    }
    public class JwtTokenService : IJwtTokenService
        {
            private readonly string _secretKey;
            private readonly string _issuer;
            private readonly string _audience;
            private readonly int _expirationMinutes;
        
        public JwtTokenService(IConfiguration cfg)
        {
            _secretKey = cfg["JwtSettings:SecretKey"]?? "CléSecretParDefautTresLonguePourLaSecurite123!";
            _issuer = cfg["JwtSettings:Issuer"] ?? "BankApp";
            _audience = cfg["JwtSettings:Audience"] ?? "BankUsers";
            _expirationMinutes = int.Parse(cfg["JwtSettings:ExperationMinutes"] ?? "1440");
        }
        public string GenerateAccessToken(int idUsers, string email, string nom_complet, string[] roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, idUsers.ToString()),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Name, nom_complet),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                    signingCredentials: creds
          );
            return new JwtSecurityTokenHandler().
                WriteToken(token);

        }
        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string GenreateRefreshToken()
        {
            throw new NotImplementedException();
        }
    }

    
}
