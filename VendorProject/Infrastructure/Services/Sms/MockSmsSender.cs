using Microsoft.Extensions.Logging;

namespace VendorProject.Infrastructure.Services.Sms
{
    /// <summary>
    /// Mock SMS provider for testing and development
    /// </summary>
    public class MockSmsSender : ISmsSender
    {
        private readonly ILogger<MockSmsSender> _logger;

        public string ProviderName => "Mock";

        public MockSmsSender(ILogger<MockSmsSender> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                // In development/testing, just log the OTP
                _logger.LogInformation("MOCK SMS to {PhoneNumber}: {Message}", phoneNumber, message);
                await Task.Delay(100, cancellationToken); // Simulate network delay
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true); // Always available in testing
        }
    }
}
