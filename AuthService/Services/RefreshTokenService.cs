
using System.Security.Cryptography;
using System.Text;
using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class RefreshTokenService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> CreateAsync(
            User user,
            string? userAgent,
            System.Net.IPAddress? ipAddress)
        {
            // Generate a cryptographically secure random token.
            var rawToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            // Store only the token hash in PostgreSQL.
            var tokenHash = HashToken(rawToken);

            var expiryDays = _configuration.GetValue<int>(
                "RefreshToken:ExpiryDays", 7);

            if (expiryDays <= 0)
            {
                throw new InvalidOperationException(
                    "Refresh token expiry must be positive.");
            }

            var now = DateTimeOffset.UtcNow;

            var refreshToken = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now.AddDays(expiryDays),
                UserAgent = userAgent,
                IpAddress = ipAddress
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // Return the raw token to the client only once.
            return rawToken;
        }

        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        }
    }
}
