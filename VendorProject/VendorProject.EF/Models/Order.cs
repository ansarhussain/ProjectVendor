namespace VendorProject.EF.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid BuyerUserId { get; set; }
        public Guid VendorUserId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        public Guid DeliveryAddressId { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User BuyerUser { get; set; } = null!;
        public User VendorUser { get; set; } = null!;
        public Address DeliveryAddress { get; set; } = null!;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }

    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Guid VendorListingId { get; set; }
        public Guid ProductId { get; set; } // denormalized for query speed

        public decimal Qty { get; set; } // decimal(18,3)
        public decimal UnitPrice { get; set; } // decimal(18,2) snapshot
        public string Currency { get; set; } = "INR";
        public decimal LineAmount { get; set; } // decimal(18,2) snapshot

        public DateTime CreatedAt { get; set; }

        public Order Order { get; set; } = null!;
        public VendorListing VendorListing { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }

    public class Payment
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Guid PayerUserId { get; set; }

        public decimal Amount { get; set; } // decimal(18,2)
        public string Currency { get; set; } = "INR";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Provider { get; set; } // Razorpay/Stripe/etc
        public string? ProviderRef { get; set; }

        public DateTime CreatedAt { get; set; }

        public Order Order { get; set; } = null!;
        public User PayerUser { get; set; } = null!;
    }
}
