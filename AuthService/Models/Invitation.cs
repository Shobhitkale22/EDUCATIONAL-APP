namespace AuthService.Models
{
    public class Invitation
    {
        public long InvitationId { get; set; }
        public long InviterUserId { get; set; }
        public short IntendedRoleId { get; set; }

        public string InviteeEmail { get; set; } = string.Empty;
        public string? NormalizedEmail { get; private set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? AcceptedAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}