using AuthService.Helpers.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Helpers.Implementations
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();
        public string HashPassword(string Password)
        {
            return _hasher.HashPassword(new object(), Password);
        }

        public bool VerifyPassword(string Password, string HashedPassword)
        {
            var result = _hasher.VerifyHashedPassword(new object(), HashedPassword, Password);
            return result == PasswordVerificationResult.Success;
            
        }
    }
}