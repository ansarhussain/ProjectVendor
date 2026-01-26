namespace VendorProject.Infrastructure.Services.Sms
{
    /// <summary>
    /// Strategy pattern interface for SMS providers.
    /// Abstracts different SMS service implementations (Twilio, Vonage, AWS SNS, etc.)
    /// </summary>
    public interface ISmsSender
    {
        /// <summary>
        /// Provider identifier
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Send OTP to phone number
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number in E.164 format</param>
        /// <param name="message">OTP message body</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendOtpAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if provider is available/configured
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}
