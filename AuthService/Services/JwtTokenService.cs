
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user)
        {
            var signingKey = _configuration["Jwt:SigningKey"]
                ?? throw new InvalidOperationException(
                    "JWT signing key is not configured.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is not configured.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is not configured.");

            var expiryMinutes = _configuration
                .GetValue<int>("Jwt:AccessTokenMinutes");

            if (expiryMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "JWT access token lifetime must be positive.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Role,
                    user.Role?.RoleName
                        ?? throw new InvalidOperationException(
                            "User role must be loaded before token creation.")),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
