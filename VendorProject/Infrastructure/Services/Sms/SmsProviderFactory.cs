using Microsoft.Extensions.Logging;
using VendorProject.EF.Models;

namespace VendorProject.Infrastructure.Services.Sms
{
    /// <summary>
    /// SMS provider factory with strategy selection and fallback support
    /// </summary>
    public interface ISmsProviderFactory
    {
        /// <summary>
        /// Get provider by type, with fallback support
        /// </summary>
        /// <param name="preferredProvider">Preferred provider, will fallback to available ones if not available</param>
        /// <returns>Available SMS sender instance</returns>
        Task<ISmsSender?> GetAvailableProviderAsync(OtpProvider preferredProvider, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get specific provider by type
        /// </summary>
        ISmsSender? GetProvider(OtpProvider provider);

        /// <summary>
        /// Get all available providers
        /// </summary>
        Task<List<(OtpProvider type, ISmsSender sender)>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of SMS provider factory
    /// </summary>
    public class SmsProviderFactory : ISmsProviderFactory
    {
        private readonly ILogger<SmsProviderFactory> _logger;
        private readonly Dictionary<OtpProvider, ISmsSender> _providers;

        public SmsProviderFactory(ILogger<SmsProviderFactory> logger, IEnumerable<ISmsSender> providers)
        {
            _logger = logger;
            _providers = new Dictionary<OtpProvider, ISmsSender>();

            // Map providers by their type
            foreach (var provider in providers)
            {
                if (provider is TwilioSmsSender)
                    _providers[OtpProvider.Twilio] = provider;
                else if (provider is VonageSmsSender)
                    _providers[OtpProvider.Vonage] = provider;
                else if (provider is AwsSnsSmsSender)
                    _providers[OtpProvider.AwsSns] = provider;
                else if (provider is MockSmsSender)
                    _providers[OtpProvider.MockProvider] = provider;
            }

            _logger.LogInformation("SMS provider factory initialized with {Count} providers", _providers.Count);
        }

        public async Task<ISmsSender?> GetAvailableProviderAsync(OtpProvider preferredProvider, CancellationToken cancellationToken = default)
        {
            // Try preferred provider first
            if (_providers.TryGetValue(preferredProvider, out var provider))
            {
                if (await provider.IsAvailableAsync(cancellationToken))
                {
                    _logger.LogDebug("Using preferred provider: {Provider}", provider.ProviderName);
                    return provider;
                }

                _logger.LogWarning("Preferred provider {Provider} not available, trying fallbacks", provider.ProviderName);
            }

            // Try other providers as fallback
            foreach (var fallback in _providers.Values)
            {
                if (await fallback.IsAvailableAsync(cancellationToken))
                {
                    _logger.LogInformation("Using fallback provider: {Provider}", fallback.ProviderName);
                    return fallback;
                }
            }

            _logger.LogError("No SMS provider available");
            return null;
        }

        public ISmsSender? GetProvider(OtpProvider provider)
        {
            _providers.TryGetValue(provider, out var result);
            return result;
        }

        public async Task<List<(OtpProvider type, ISmsSender sender)>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default)
        {
            var available = new List<(OtpProvider, ISmsSender)>();

            foreach (var (type, sender) in _providers)
            {
                if (await sender.IsAvailableAsync(cancellationToken))
                {
                    available.Add((type, sender));
                }
            }

            return available;
        }
    }
}
