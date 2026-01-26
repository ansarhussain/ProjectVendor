namespace VendorProject.EF.Models
{
    public class UserOtp
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string PhoneNumber { get; set; } = null!; // Denormalized for OTP lookup
        public string OtpCode { get; set; } = null!; // 6-digit code
        public OtpProvider Provider { get; set; } = OtpProvider.Twilio; // SMS provider used
        
        public OtpPurpose Purpose { get; set; } // Registration, Login, PasswordReset
        public bool IsVerified { get; set; } = false;
        
        public int AttemptCount { get; set; } = 0;
        public int MaxAttempts { get; set; } = 3;
        
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; } // Typically 10 minutes
        public DateTime? VerifiedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }

    public enum OtpProvider
    {
        Twilio,
        Vonage,
        AwsSns,
        MockProvider // For testing
    }

    public enum OtpPurpose
    {
        Registration,
        Login,
        PasswordReset,
        PhoneVerification
    }
}
