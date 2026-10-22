
using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Invitation> Invitations => Set<Invitation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(x => x.RoleId);

                entity.Property(x => x.RoleId)
                    .HasColumnName("role_id");

                entity.Property(x => x.RoleName)
                    .HasColumnName("role_name")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(x => x.UserId);

                entity.Property(x => x.UserId)
                    .HasColumnName("user_id");

                entity.Property(x => x.RoleId)
                    .HasColumnName("role_id");

                entity.Property(x => x.FullName)
                    .HasColumnName("full_name");

                entity.Property(x => x.Email)
                    .HasColumnName("email");

                entity.Property(x => x.NormalizedEmail)
                    .HasColumnName("normalized_email")
                    .HasComputedColumnSql(
                        "lower(btrim(email))", stored: true);

                entity.Property(x => x.PasswordHash)
                    .HasColumnName("password_hash");

                entity.Property(x => x.Phone)
                    .HasColumnName("phone");

                entity.Property(x => x.IsActive)
                    .HasColumnName("is_active");

                entity.Property(x => x.IsBlocked)
                    .HasColumnName("is_blocked");

                entity.Property(x => x.EmailVerifiedAt)
                    .HasColumnName("email_verified_at");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(x => x.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(x => x.LastLoginAt)
                    .HasColumnName("last_login_at");

                entity.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId);
            });

            // Refresh tokens
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(x => x.RefreshTokenId);

                entity.Property(x => x.RefreshTokenId)
                    .HasColumnName("refresh_token_id");

                entity.Property(x => x.UserId)
                    .HasColumnName("user_id");

                entity.Property(x => x.TokenHash)
                    .HasColumnName("token_hash");

                entity.Property(x => x.ExpiresAt)
                    .HasColumnName("expires_at");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(x => x.RevokedAt)
                    .HasColumnName("revoked_at");

                entity.Property(x => x.ReplacedByTokenId)
                    .HasColumnName("replaced_by_token_id");

                entity.Property(x => x.UserAgent)
                    .HasColumnName("user_agent");

                entity.Property(x => x.IpAddress)
                    .HasColumnName("ip_address")
                    .HasColumnType("inet");

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId);

                entity.HasOne(x => x.ReplacedByToken)
                    .WithMany()
                    .HasForeignKey(x => x.ReplacedByTokenId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Invitations
            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.ToTable("invitations");
                entity.HasKey(x => x.InvitationId);

                entity.Property(x => x.InvitationId)
                    .HasColumnName("invitation_id");

                entity.Property(x => x.InviterUserId)
                    .HasColumnName("inviter_user_id");

                entity.Property(x => x.IntendedRoleId)
                    .HasColumnName("intended_role_id");

                entity.Property(x => x.InviteeEmail)
                    .HasColumnName("invitee_email");

                entity.Property(x => x.NormalizedEmail)
                    .HasColumnName("normalized_email")
                    .HasComputedColumnSql(
                        "lower(btrim(invitee_email))", stored: true);

                entity.Property(x => x.TokenHash)
                    .HasColumnName("token_hash");

                entity.Property(x => x.ExpiresAt)
                    .HasColumnName("expires_at");

                entity.Property(x => x.AcceptedAt)
                    .HasColumnName("accepted_at");

                entity.Property(x => x.RevokedAt)
                    .HasColumnName("revoked_at");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at");
            });
            }
    }
}
