namespace AuthService.Models
{
    public class User
    {
        public long UserId { get; set; }
        public short RoleId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? NormalizedEmail { get; private set; }
        public string? PasswordHash { get; set; }
        public string? Phone { get; set; }

        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }

        public DateTimeOffset? EmailVerifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }

        public Role? Role { get; set; }
    }
}