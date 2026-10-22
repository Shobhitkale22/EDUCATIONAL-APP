using System.Net;

namespace AuthService.Models
{
    public class RefreshToken
    {
        public long RefreshTokenId { get; set; }
        public long UserId { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }

        public long? ReplacedByTokenId { get; set; }
        public string? UserAgent { get; set; }
        public IPAddress? IpAddress { get; set; }

        public User? User { get; set; }
        public RefreshToken? ReplacedByToken { get; set; }
    }
}