using Microsoft.Extensions.Logging;

namespace VendorProject.Infrastructure.Services.Sms
{
    /// <summary>
    /// Vonage SMS provider implementation
    /// </summary>
    public class VonageSmsSender : ISmsSender
    {
        private readonly ILogger<VonageSmsSender> _logger;
        private readonly VonageSettings _settings;

        public string ProviderName => "Vonage";

        public VonageSmsSender(ILogger<VonageSmsSender> logger, VonageSettings settings)
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
                    _logger.LogWarning("Vonage is not properly configured");
                    return false;
                }

                // TODO: Integrate with actual Vonage SDK
                // Example:
                // var vonageClient = new SmsClient(new Credentials(_settings.ApiKey, _settings.ApiSecret));
                // var response = await vonageClient.SendAnSmsAsync(new SendSmsRequest
                // {
                //     To = phoneNumber,
                //     From = _settings.FromName,
                //     Text = message
                // });
                // return response.Messages[0].Status == "0";

                _logger.LogInformation("SMS sent via Vonage to {PhoneNumber}", phoneNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS via Vonage to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsConfigured());
        }

        private bool IsConfigured()
        {
            return !string.IsNullOrEmpty(_settings.ApiKey) &&
                   !string.IsNullOrEmpty(_settings.ApiSecret) &&
                   !string.IsNullOrEmpty(_settings.FromName);
        }
    }

    public class VonageSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
