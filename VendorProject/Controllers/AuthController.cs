using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorProject.EF.Models;
using VendorProject.Infrastructure.Services.Auth;

namespace VendorProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IOtpService _otpService;
        private readonly IJwtTokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authService,
            IOtpService otpService,
            IJwtTokenService tokenService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _otpService = otpService;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Step 1: Register new user - Send OTP to phone
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<RegistrationResponse>), 200)]
        public async Task<IActionResult> Register(
            [FromBody] RegistrationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var response = await _authService.RegisterAsync(request, cancellationToken);
                return Ok(new ApiResponse<RegistrationResponse>
                {
                    Success = response.Success,
                    Message = response.Message,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        /// <summary>
        /// Step 2: Verify registration OTP and create account
        /// </summary>
        [HttpPost("verify-registration")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        public async Task<IActionResult> VerifyRegistration(
            [FromBody] VerifyOtpRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                // Verify OTP
                var (isValid, message) = await _otpService.VerifyOtpAsync(
                    request.PhoneNumber,
                    request.OtpCode,
                    OtpPurpose.Registration,
                    cancellationToken);

                if (!isValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = message
                    });
                }

                // Create user (if not already created)
                // TODO: This should be part of a separate user creation flow post-OTP verification
                // For now, we'll assume user already exists or needs to be created

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Phone number verified successfully. Please complete your profile."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyRegistration endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while verifying OTP"
                });
            }
        }

        /// <summary>
        /// Step 1: Initiate login - Send OTP to phone
        /// </summary>
        [HttpPost("send-login-otp")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> SendLoginOtp(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var response = await _authService.LoginAsync(request.PhoneNumber, cancellationToken);
                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = response.Success,
                    Message = response.Message,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendLoginOtp endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while sending OTP"
                });
            }
        }

        /// <summary>
        /// Step 2: Verify login OTP and get tokens
        /// </summary>
        [HttpPost("verify-login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> VerifyLogin(
            [FromBody] VerifyLoginRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                // Verify OTP
                var (isValid, message) = await _otpService.VerifyOtpAsync(
                    request.PhoneNumber,
                    request.OtpCode,
                    OtpPurpose.Login,
                    cancellationToken);

                if (!isValid)
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = message
                    });
                }

                // Complete login
                var response = await _authService.CompleteLoginAsync(request.PhoneNumber, cancellationToken);
                
                if (!response.Success)
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = response.Message
                    });
                }

                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = response.Message,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyLogin endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while verifying login"
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenResponse>), 200)]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }

                var response = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

                if (response == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    });
                }

                return Ok(new ApiResponse<TokenResponse>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefreshToken endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while refreshing token"
                });
            }
        }

        /// <summary>
        /// Logout - Revoke all refresh tokens
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid user"
                    });
                }

                await _authService.LogoutAsync(userId, cancellationToken);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Logout endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid user"
                    });
                }

                var user = await _authService.GetUserAsync(userId, cancellationToken);

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User profile retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProfile endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving profile"
                });
            }
        }
    }

    // Request/Response DTOs
    public class VerifyOtpRequest
    {
        public string PhoneNumber { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }

    public class VerifyLoginRequest
    {
        public string PhoneNumber { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = null!;
    }

    // Generic API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
