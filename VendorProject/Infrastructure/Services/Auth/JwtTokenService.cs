using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using VendorProject.EF.Data;
using VendorProject.EF.Models;

namespace VendorProject.Infrastructure.Services.Auth
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "VendorProject";
        public string Audience { get; set; } = "VendorProjectUsers";
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int ExpiresIn { get; set; } // In seconds
        public string TokenType { get; set; } = "Bearer";
    }

    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate JWT access token
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generate and store refresh token
        /// </summary>
        Task<string> GenerateRefreshTokenAsync(Guid userId, string jwtTokenId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate and extract claims from JWT token
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Verify refresh token is valid and not revoked
        /// </summary>
        Task<bool> VerifyRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate complete token response
        /// </summary>
        Task<TokenResponse> GenerateTokenResponseAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clean up expired refresh tokens
        /// </summary>
        Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly MarketplaceDbContext _dbContext;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly JwtSettings _settings;
        private readonly SecurityKey _securityKey;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(
            MarketplaceDbContext dbContext,
            ILogger<JwtTokenService> logger,
            JwtSettings settings)
        {
            _dbContext = dbContext;
            _logger = logger;
            _settings = settings;

            // Initialize security key and signing credentials
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
            _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
        }

        public string GenerateAccessToken(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim("Phone", user.Phone),
                    new Claim("PhoneVerified", user.IsPhoneVerified.ToString()),
                    new Claim("KycVerified", user.IsKycVerified.ToString()),
                };

                // Add role claims
                var roles = user.Roles.Select(r => r.Role.ToString()).ToList();
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var now = DateTime.UtcNow;
                var expiration = now.AddMinutes(_settings.AccessTokenExpirationMinutes);

                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: expiration,
                    signingCredentials: _signingCredentials)
                {
                    // jti (JWT ID) claim for refresh token linking
                    Payload = { { "jti", Guid.NewGuid().ToString() } }
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var accessToken = tokenHandler.WriteToken(token);

                _logger.LogInformation("Access token generated for user {UserId}", user.Id);
                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<string> GenerateRefreshTokenAsync(Guid userId, string jwtTokenId, CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                var now = DateTime.UtcNow;

                var token = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = refreshToken,
                    JwtTokenId = jwtTokenId,
                    CreatedAt = now,
                    ExpiresAt = now.AddDays(_settings.RefreshTokenExpirationDays)
                };

                _dbContext.RefreshTokens.Add(token);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Refresh token generated for user {UserId}", userId);
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token for user {UserId}", userId);
                throw;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public async Task<bool> VerifyRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var token = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

                if (token == null)
                {
                    _logger.LogWarning("Refresh token not found in database");
                    return false;
                }

                if (token.IsRevoked)
                {
                    _logger.LogWarning("Refresh token has been revoked");
                    return false;
                }

                if (token.IsExpired)
                {
                    _logger.LogWarning("Refresh token has expired");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying refresh token");
                return false;
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var token = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

                if (token != null)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Refresh token revoked");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                throw;
            }
        }

        public async Task<TokenResponse> GenerateTokenResponseAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = GenerateAccessToken(user);
                var jwtTokenId = ExtractJtiClaim(accessToken);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id, jwtTokenId, cancellationToken);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(accessToken);

                return new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = (int)(jwtToken.ValidTo - DateTime.UtcNow).TotalSeconds,
                    TokenType = "Bearer"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token response for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var expiredTokens = await _dbContext.RefreshTokens
                    .Where(t => DateTime.UtcNow > t.ExpiresAt)
                    .ToListAsync(cancellationToken);

                _dbContext.RefreshTokens.RemoveRange(expiredTokens);
                var deletedCount = await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Cleaned up {Count} expired refresh tokens", deletedCount);
                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
                throw;
            }
        }

        private string ExtractJtiClaim(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? Guid.NewGuid().ToString();
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }
    }
}
