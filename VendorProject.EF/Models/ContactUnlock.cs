namespace VendorProject.EF.Models
{
    public class ContactUnlock
    {
        public Guid Id { get; set; }

        public Guid BuyerUserId { get; set; }
        public Guid TargetUserId { get; set; }

        public Guid? OrderId { get; set; }

        public DateTime UnlockedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public string UnlockReason { get; set; } = "Payment";
        public DateTime CreatedAt { get; set; }

        public User BuyerUser { get; set; } = null!;
        public User TargetUser { get; set; } = null!;
        public Order? Order { get; set; }
    }
}
