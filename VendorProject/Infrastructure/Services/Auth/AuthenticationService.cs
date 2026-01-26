using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendorProject.EF.Data;
using VendorProject.EF.Models;

namespace VendorProject.Infrastructure.Services.Auth
{
    public class RegistrationRequest
    {
        public string PhoneNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public UserRoleType DesiredRole { get; set; } = UserRoleType.Buyer;
    }

    public class RegistrationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public Guid? UserId { get; set; }
        public UserDto? User { get; set; }
    }

    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = null!;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public TokenResponse? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsKycVerified { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public interface IAuthenticationService
    {
        /// <summary>
        /// Register a new user with phone-based OTP
        /// </summary>
        Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Login user with OTP verification
        /// </summary>
        Task<LoginResponse> LoginAsync(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Complete login after OTP verification
        /// </summary>
        Task<LoginResponse> CompleteLoginAsync(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        Task<TokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logout user (revoke refresh tokens)
        /// </summary>
        Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user details by ID
        /// </summary>
        Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly MarketplaceDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IOtpService _otpService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            MarketplaceDbContext dbContext,
            IJwtTokenService jwtTokenService,
            IOtpService otpService,
            ILogger<AuthenticationService> logger)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _otpService = otpService;
            _logger = logger;
        }

        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Length < 10)
                {
                    return new RegistrationResponse
                    {
                        Success = false,
                        Message = "Invalid phone number"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return new RegistrationResponse
                    {
                        Success = false,
                        Message = "Full name is required"
                    };
                }

                // Check if user already exists
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Phone == request.PhoneNumber, cancellationToken);

                if (existingUser != null)
                {
                    return new RegistrationResponse
                    {
                        Success = false,
                        Message = "User already registered with this phone number",
                        UserId = existingUser.Id,
                        User = MapToUserDto(existingUser)
                    };
                }

                // Generate and send OTP
                var (success, message, otpCode) = await _otpService.GenerateAndSendOtpAsync(
                    request.PhoneNumber,
                    OtpPurpose.Registration,
                    cancellationToken: cancellationToken);

                if (!success)
                {
                    return new RegistrationResponse
                    {
                        Success = false,
                        Message = message
                    };
                }

                _logger.LogInformation("Registration OTP sent for phone {Phone}", request.PhoneNumber);

                // Return OTP for testing (only when using mock provider)
                return new RegistrationResponse
                {
                    Success = true,
                    Message = "OTP sent successfully. Please verify to complete registration.",
                    User = new UserDto { Phone = request.PhoneNumber, FullName = request.FullName }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with phone {Phone}", request.PhoneNumber);
                return new RegistrationResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<LoginResponse> LoginAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate phone number
                if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid phone number"
                    };
                }

                // Check if user exists
                var user = await _dbContext.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Phone == phoneNumber && u.IsActive, cancellationToken);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User not found with this phone number"
                    };
                }

                // Generate and send OTP
                var (success, message, otpCode) = await _otpService.GenerateAndSendOtpAsync(
                    phoneNumber,
                    OtpPurpose.Login,
                    user.Id,
                    cancellationToken);

                if (!success)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = message
                    };
                }

                _logger.LogInformation("Login OTP sent for phone {Phone}", phoneNumber);

                return new LoginResponse
                {
                    Success = true,
                    Message = "OTP sent successfully. Please verify to login.",
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating login for phone {Phone}", phoneNumber);
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<LoginResponse> CompleteLoginAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get user
                var user = await _dbContext.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Phone == phoneNumber && u.IsActive, cancellationToken);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Check if phone is verified
                if (!user.IsPhoneVerified)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Phone number not verified. Please complete registration first."
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Generate tokens
                var tokenResponse = await _jwtTokenService.GenerateTokenResponseAsync(user, cancellationToken);

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = tokenResponse,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing login for phone {Phone}", phoneNumber);
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // Verify refresh token
                if (!await _jwtTokenService.VerifyRefreshTokenAsync(refreshToken, cancellationToken))
                {
                    _logger.LogWarning("Invalid refresh token attempt");
                    return null;
                }

                // Get associated user
                var tokenRecord = await _dbContext.RefreshTokens
                    .Include(t => t.User)
                    .ThenInclude(u => u.Roles)
                    .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

                if (tokenRecord?.User == null)
                {
                    return null;
                }

                // Revoke old token
                await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);

                // Generate new tokens
                var newTokenResponse = await _jwtTokenService.GenerateTokenResponseAsync(tokenRecord.User, cancellationToken);

                _logger.LogInformation("Token refreshed for user {UserId}", tokenRecord.User.Id);
                return newTokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return null;
            }
        }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Revoke all refresh tokens for user
                var userTokens = await _dbContext.RefreshTokens
                    .Where(t => t.UserId == userId && !t.IsRevoked)
                    .ToListAsync(cancellationToken);

                foreach (var token in userTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("User {UserId} logged out", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                return user == null ? null : MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return null;
            }
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                IsPhoneVerified = user.IsPhoneVerified,
                IsKycVerified = user.IsKycVerified,
                Roles = user.Roles.Select(r => r.Role.ToString()).ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
