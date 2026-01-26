namespace VendorProject.EF.Models
{
    public class UserDevice
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string DeviceName { get; set; } = null!; // e.g., "iPhone 14", "Samsung Galaxy S21"
        public string DeviceId { get; set; } = null!; // Unique device fingerprint/IMEI
        public string DeviceType { get; set; } = null!; // Mobile, Tablet, Web

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
