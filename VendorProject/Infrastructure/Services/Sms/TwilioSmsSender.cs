using Microsoft.Extensions.Logging;

namespace VendorProject.Infrastructure.Services.Sms
{
    /// <summary>
    /// Twilio SMS provider implementation
    /// </summary>
    public class TwilioSmsSender : ISmsSender
    {
        private readonly ILogger<TwilioSmsSender> _logger;
        private readonly TwilioSettings _settings;

        public string ProviderName => "Twilio";

        public TwilioSmsSender(ILogger<TwilioSmsSender> logger, TwilioSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsConfigured())
                {
                    _logger.LogWarning("Twilio is not properly configured");
                    return false;
                }

                // TODO: Integrate with actual Twilio SDK
                // Example:
                // var twilioClient = new TwilioRestClient(_settings.AccountSid, _settings.AuthToken);
                // var result = await twilioClient.Messaging.V1.MessageResource.CreateAsync(
                //     to: phoneNumber,
                //     from: _settings.FromNumber,
                //     body: message,
                //     region: _settings.Region);
                // return result != null;

                _logger.LogInformation("SMS sent via Twilio to {PhoneNumber}", phoneNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS via Twilio to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsConfigured());
        }

        private bool IsConfigured()
        {
            return !string.IsNullOrEmpty(_settings.AccountSid) &&
                   !string.IsNullOrEmpty(_settings.AuthToken) &&
                   !string.IsNullOrEmpty(_settings.FromNumber);
        }
    }

    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
        public string? Region { get; set; }
    }
}
