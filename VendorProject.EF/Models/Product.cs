namespace VendorProject.EF.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }

        public string Unit { get; set; } = "UNIT"; // KG/TON/BAG etc
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public ICollection<VendorListing> VendorListings { get; set; } = new List<VendorListing>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class VendorListing
    {
        public Guid Id { get; set; }

        public Guid VendorUserId { get; set; }
        public Guid ProductId { get; set; }

        public decimal AvailableQty { get; set; } // decimal(18,3)
        public decimal MinOrderQty { get; set; }  // decimal(18,3)

        public decimal PricePerUnit { get; set; } // decimal(18,2)
        public string Currency { get; set; } = "INR";

        public DateTime? PriceValidFrom { get; set; }
        public DateTime? PriceValidTo { get; set; }

        public Guid? ListingAddressId { get; set; } // warehouse

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public User VendorUser { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Address? ListingAddress { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
