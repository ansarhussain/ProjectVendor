using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendorProject.EF.Data;
using VendorProject.EF.Models;
using VendorProject.Infrastructure.Services.Sms;

namespace VendorProject.Infrastructure.Services.Auth
{
    public class OtpSettings
    {
        public int OtpLength { get; set; } = 6;
        public int OtpValidityMinutes { get; set; } = 10;
        public int MaxAttempts { get; set; } = 3;
        public int RateLimitRequestsPerMinute { get; set; } = 3;
    }

    public interface IOtpService
    {
        /// <summary>
        /// Generate and send OTP to phone number
        /// </summary>
        Task<(bool success, string message, string? otpCode)> GenerateAndSendOtpAsync(
            string phoneNumber,
            OtpPurpose purpose,
            Guid? userId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify OTP code
        /// </summary>
        Task<(bool isValid, string message)> VerifyOtpAsync(
            string phoneNumber,
            string otpCode,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get latest OTP for a phone number (for testing)
        /// </summary>
        Task<UserOtp?> GetLatestOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clean up expired OTPs
        /// </summary>
        Task<int> CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default);
    }

    public class OtpService : IOtpService
    {
        private readonly MarketplaceDbContext _dbContext;
        private readonly ISmsProviderFactory _providerFactory;
        private readonly ILogger<OtpService> _logger;
        private readonly OtpSettings _settings;

        public OtpService(
            MarketplaceDbContext dbContext,
            ISmsProviderFactory providerFactory,
            ILogger<OtpService> logger,
            OtpSettings settings)
        {
            _dbContext = dbContext;
            _providerFactory = providerFactory;
            _logger = logger;
            _settings = settings;
        }

        public async Task<(bool success, string message, string? otpCode)> GenerateAndSendOtpAsync(
            string phoneNumber,
            OtpPurpose purpose,
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate phone number
                if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10)
                {
                    return (false, "Invalid phone number", null);
                }

                // Rate limiting check
                var recentOtps = await _dbContext.UserOtps
                    .Where(o => o.PhoneNumber == phoneNumber && 
                                o.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
                    .CountAsync(cancellationToken);

                if (recentOtps >= _settings.RateLimitRequestsPerMinute)
                {
                    _logger.LogWarning("Rate limit exceeded for phone: {Phone}", phoneNumber);
                    return (false, "Too many OTP requests. Please try after 1 minute.", null);
                }

                // Invalidate previous unverified OTPs for this phone/purpose
                var previousOtps = await _dbContext.UserOtps
                    .Where(o => o.PhoneNumber == phoneNumber && 
                                o.Purpose == purpose && 
                                !o.IsVerified)
                    .ToListAsync(cancellationToken);

                _dbContext.UserOtps.RemoveRange(previousOtps);

                // Generate OTP
                var otpCode = GenerateRandomOtp();
                var now = DateTime.UtcNow;

                var userOtp = new UserOtp
                {
                    Id = Guid.NewGuid(),
                    UserId = userId ?? Guid.Empty,
                    PhoneNumber = phoneNumber,
                    OtpCode = otpCode,
                    Purpose = purpose,
                    Provider = OtpProvider.Twilio, // Default to Twilio, will use factory for actual sending
                    IsVerified = false,
                    AttemptCount = 0,
                    MaxAttempts = _settings.MaxAttempts,
                    CreatedAt = now,
                    ExpiresAt = now.AddMinutes(_settings.OtpValidityMinutes)
                };

                _dbContext.UserOtps.Add(userOtp);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Send OTP via SMS
                var smsMessage = $"Your OTP is: {otpCode}. Valid for {_settings.OtpValidityMinutes} minutes.";
                var provider = await _providerFactory.GetAvailableProviderAsync(OtpProvider.Twilio, cancellationToken);

                if (provider == null)
                {
                    _logger.LogError("No SMS provider available for OTP");
                    return (false, "SMS service temporarily unavailable. Please try again later.", null);
                }

                var smsSent = await provider.SendOtpAsync(phoneNumber, smsMessage, cancellationToken);

                if (!smsSent)
                {
                    _logger.LogError("Failed to send SMS to {Phone}", phoneNumber);
                    return (false, "Failed to send OTP. Please try again.", null);
                }

                // Update provider in DB
                userOtp.Provider = provider.ProviderName switch
                {
                    "Twilio" => OtpProvider.Twilio,
                    "Vonage" => OtpProvider.Vonage,
                    "AwsSns" => OtpProvider.AwsSns,
                    "Mock" => OtpProvider.MockProvider,
                    _ => OtpProvider.Twilio
                };

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("OTP sent successfully to {Phone} for purpose {Purpose}", phoneNumber, purpose);
                
                // Return OTP only in development/testing (when using Mock provider)
                var returnOtp = provider.ProviderName == "Mock" ? otpCode : null;
                return (true, "OTP sent successfully", returnOtp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and sending OTP for {Phone}", phoneNumber);
                return (false, "An error occurred. Please try again.", null);
            }
        }

        public async Task<(bool isValid, string message)> VerifyOtpAsync(
            string phoneNumber,
            string otpCode,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userOtp = await _dbContext.UserOtps
                    .Where(o => o.PhoneNumber == phoneNumber &&
                                o.Purpose == purpose &&
                                !o.IsVerified)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (userOtp == null)
                {
                    return (false, "No valid OTP found for this phone number.");
                }

                // Check if expired
                if (DateTime.UtcNow > userOtp.ExpiresAt)
                {
                    return (false, "OTP has expired. Please request a new one.");
                }

                // Check max attempts
                if (userOtp.AttemptCount >= userOtp.MaxAttempts)
                {
                    return (false, $"Maximum attempts exceeded. Please request a new OTP.");
                }

                // Increment attempt
                userOtp.AttemptCount++;

                // Verify code
                if (userOtp.OtpCode != otpCode)
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    var remainingAttempts = userOtp.MaxAttempts - userOtp.AttemptCount;
                    return (false, $"Invalid OTP. {remainingAttempts} attempts remaining.");
                }

                // Mark as verified
                userOtp.IsVerified = true;
                userOtp.VerifiedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("OTP verified successfully for phone {Phone}", phoneNumber);
                return (true, "OTP verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Phone}", phoneNumber);
                return (false, "An error occurred while verifying OTP.");
            }
        }

        public async Task<UserOtp?> GetLatestOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserOtps
                .Where(o => o.PhoneNumber == phoneNumber)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int> CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default)
        {
            var expiredOtps = await _dbContext.UserOtps
                .Where(o => DateTime.UtcNow > o.ExpiresAt && !o.IsVerified)
                .ToListAsync(cancellationToken);

            _dbContext.UserOtps.RemoveRange(expiredOtps);
            var deletedCount = await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleaned up {Count} expired OTPs", deletedCount);
            return deletedCount;
        }

        private string GenerateRandomOtp()
        {
            var random = new Random();
            var otp = string.Empty;

            for (int i = 0; i < _settings.OtpLength; i++)
            {
                otp += random.Next(0, 10).ToString();
            }

            return otp;
        }
    }
}
