using System;
using BC = BCrypt.Net.BCrypt;

namespace Bank_Slip_Scanner_App.Services
{
    public interface IPasswordHasher
    {
        (string hash, string salt) HashPassword(string password);
        bool verify(string oldPassword, string motDePasseHash);
        bool Verify(string password, object motDePasseHash);
        bool VerifyPassword(string password, string hash);
    }
    public class PasswordHasher : IPasswordHasher
    {
   

        ///<summary>
        ///hash un mot de passe avec BCrypt (salt auto-généré)

        ///</summary>
        public (string hash, string salt) HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) 
                throw new ArgumentNullException("password cannot be empty");
             string hash = BC.HashPassword(password, workFactor: 12);
             string salt = hash.Substring(0, 29);
             return (hash, salt);
            }

        public bool verify(string oldPassword, string motDePasseHash)
        {
            throw new NotImplementedException();
        }

        public bool Verify(string password, object motDePasseHash)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///verifie  mot de passe 

        ///</summary>
        public bool VerifyPassword(string password, string hash) {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash)) 
                return false;
                try
                {
                return BC.Verify(password, hash);
                }
                catch {
                    return false;
                }

            }
    }
    }


