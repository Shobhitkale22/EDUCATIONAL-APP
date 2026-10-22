using Microsoft.AspNetCore.Identity;

namespace AuthService.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string HashPassword(string password)
        {
            return _hasher.HashPassword(new object(), password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var result = _hasher.VerifyHashedPassword(
                new object(),
                passwordHash,
                password
            );

            return result != PasswordVerificationResult.Failed;
        }
    }
}