namespace VendorProject.EF.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Token { get; set; } = null!;
        public string JwtTokenId { get; set; } = null!; // jti claim for revocation

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public bool IsRevoked => RevokedAt != null;
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsRevoked && !IsExpired;

        // Navigation
        public User User { get; set; } = null!;
    }
}
