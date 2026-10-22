
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;


namespace AuthService.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;
        private readonly RefreshTokenService _refreshTokenService;
        

        public AuthService(
            AppDbContext context,
            PasswordService passwordService,
            JwtTokenService jwtTokenService,
            IConfiguration configuration,
            RefreshTokenService refreshTokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
        }

        // ============================================
        // 1. LOGIN
        // ============================================
        public async Task<(
            bool Success,
            string Message,
            LoginResponse? Data)> LoginAsync(LoginRequest request)
        {
            var platform = request.Platform.Trim();

            if (platform != "Web" && platform != "Mobile")
            {
                return (false, "Invalid platform.", null);
            }

            var normalizedEmail =
                request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(
                    u => u.NormalizedEmail == normalizedEmail);

            if (user == null ||
                string.IsNullOrEmpty(user.PasswordHash))
            {
                return (false, "Invalid email or password.", null);
            }

            if (!_passwordService.VerifyPassword(
                    request.Password, user.PasswordHash))
            {
                return (false, "Invalid email or password.", null);
            }

            if (user.IsBlocked)
            {
                return (false, "Your account is blocked.", null);
            }

            if (!user.IsActive)
            {
                return (false, "Your account is inactive.", null);
            }

            var roleName = user.Role?.RoleName;

            if (roleName == null)
            {
                return (false, "User role is not configured.", null);
            }

            // Only Students can use the mobile application.
            if (platform == "Mobile" && roleName != "Student")
            {
                return (
                    false,
                    "Only students can log in to the mobile application.",
                    null);
            }

            var now = DateTimeOffset.UtcNow;

            user.LastLoginAt = now;
            user.UpdatedAt = now;

            await _context.SaveChangesAsync();

            var accessToken =
                _jwtTokenService.GenerateAccessToken(user);

            var refreshToken =
                await _refreshTokenService.CreateAsync(
                    user,
                    null,
                    null);

            var expiresInMinutes =
                _configuration.GetValue<int>(
                    "Jwt:AccessTokenMinutes");

            return (
                true,
                "Login successful.",
                new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresInMinutes = expiresInMinutes,
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roleName
                });
        }

        // ============================================
        // 2. STUDENT REGISTRATION
        // ============================================
        public async Task<(bool Success, string Message)>
            RegisterStudentAsync(RegisterRequest request)
        {
            var email = request.Email.Trim();
            var normalizedEmail = email.ToLowerInvariant();

            var existingUser = await _context.Users
                .AnyAsync(
                    u => u.NormalizedEmail == normalizedEmail);

            if (existingUser)
            {
                return (
                    false,
                    "An account with this email already exists.");
            }

            var studentRole = await _context.Roles
                .SingleOrDefaultAsync(
                    r => r.RoleName == "Student");

            if (studentRole == null)
            {
                return (
                    false,
                    "Student role is not configured.");
            }

            var now = DateTimeOffset.UtcNow;

            var user = new User
            {
                RoleId = studentRole.RoleId,
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = _passwordService.HashPassword(
                    request.Password),
                Phone = string.IsNullOrWhiteSpace(request.Phone)
                    ? null
                    : request.Phone.Trim(),
                IsActive = true,
                IsBlocked = false,
                EmailVerifiedAt = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _context.Entry(user).State =
                    EntityState.Detached;

                var duplicateNowExists = await _context.Users
                    .AnyAsync(
                        u => u.NormalizedEmail == normalizedEmail);

                if (duplicateNowExists)
                {
                    return (
                        false,
                        "An account with this email already exists.");
                }

                throw;
            }

            return (
                true,
                "Student registered successfully.");
        }



        public async Task<(bool Success, string Message, LoginResponse? Data)>
            RefreshAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return (false, "Invalid refresh token.", null);
            }

            var now = DateTimeOffset.UtcNow;
            var tokenHash = RefreshTokenService.HashToken(
                request.RefreshToken.Trim());

            // Find the token by its hash. Never look up the raw token.
            var storedToken = await _context.RefreshTokens
                .AsNoTracking()
                .Include(t => t.User)
                    .ThenInclude(u => u.Role)
                .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (storedToken == null ||
                storedToken.RevokedAt != null ||
                storedToken.ExpiresAt <= now)
            {
                return (false, "Invalid or expired refresh token.", null);
            }

            var user = storedToken.User;

            if (user == null || user.IsBlocked || !user.IsActive)
            {
                return (false, "Your account is blocked or inactive.", null);
            }

            if (user.Role == null)
            {
                return (false, "User role is not configured.", null);
            }

            // Use a transaction so revocation and replacement are atomic.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // Atomically claim this token. If another request has already
            // used it, this update affects zero rows.
            var claimed = await _context.RefreshTokens
                .Where(t =>
                    t.RefreshTokenId == storedToken.RefreshTokenId &&
                    t.RevokedAt == null &&
                    t.ExpiresAt > now)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(t => t.RevokedAt, now));

            if (claimed != 1)
            {
                return (false, "Invalid or expired refresh token.", null);
            }

            // Create and persist a replacement refresh token.
            var newRefreshToken = await _refreshTokenService.CreateAsync(
                user,
                null,
                null);

            var newTokenHash = RefreshTokenService.HashToken(newRefreshToken);

            var newTokenId = await _context.RefreshTokens
                .Where(t => t.TokenHash == newTokenHash)
                .Select(t => t.RefreshTokenId)
                .SingleAsync();

            // Link the old token to its replacement.

            await _context.RefreshTokens
  .Where(t => t.RefreshTokenId == storedToken.RefreshTokenId)
  .ExecuteUpdateAsync(update => update
      .SetProperty(t => t.ReplacedByTokenId, newTokenId));



            await transaction.CommitAsync();

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var expiresInMinutes =
                _configuration.GetValue<int>("Jwt:AccessTokenMinutes");

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                TokenType = "Bearer",
                ExpiresInMinutes = expiresInMinutes,
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.RoleName
            };

            return (true, "Token refreshed successfully.", response);
        }



        public async Task<(bool Success, string Message)> LogoutAsync(
            LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return (false, "Refresh token is required.");
            }

            var tokenHash = RefreshTokenService.HashToken(
                request.RefreshToken.Trim());

            var now = DateTimeOffset.UtcNow;

            // Revoke the presented refresh token if it is still active.
            // Repeated logout requests are harmless.
            await _context.RefreshTokens
                .Where(t =>
                    t.TokenHash == tokenHash &&
                    t.RevokedAt == null)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(t => t.RevokedAt, now));

            return (true, "Logout successful.");
        }


    }
}
