namespace VendorProject.EF.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;

        public string? PasswordHash { get; set; } // if not using external auth/Identity
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public ICollection<UserContact> Contacts { get; set; } = new List<UserContact>();
        public ICollection<UserKyc> Kycs { get; set; } = new List<UserKyc>();

        public ICollection<VendorListing> VendorListings { get; set; } = new List<VendorListing>();
        public ICollection<Order> BuyerOrders { get; set; } = new List<Order>();
        public ICollection<Order> VendorOrders { get; set; } = new List<Order>();

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<TransportRoute> TransportRoutes { get; set; } = new List<TransportRoute>();
        public ICollection<Shipment> ShipmentsAsTransporter { get; set; } = new List<Shipment>();

        public ICollection<Payment> PaymentsAsPayer { get; set; } = new List<Payment>();

        public ICollection<ContactUnlock> ContactUnlocksAsBuyer { get; set; } = new List<ContactUnlock>();
        public ICollection<ContactUnlock> ContactUnlocksAsTarget { get; set; } = new List<ContactUnlock>();
    }

    public class UserRole
    {
        public Guid UserId { get; set; }
        public UserRoleType Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }

    public class UserKyc
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string KycType { get; set; } = null!;   // Aadhaar/PAN/Passport etc.
        public string KycNumber { get; set; } = null!; // store encrypted/masked in real systems

        public KycStatus VerifiedStatus { get; set; } = KycStatus.Pending;
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }

    public class UserContact
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public ContactType ContactType { get; set; } = ContactType.Primary;

        public string? Name { get; set; }
        public string Phone { get; set; } = null!;
        public bool IsPublic { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
