namespace VendorProject.EF.Models
{
    public class Address
    {
        public Guid Id { get; set; }

        public string Line1 { get; set; } = null!;
        public string? Line2 { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = "India";

        public decimal? Latitude { get; set; }   // decimal(9,6)
        public decimal? Longitude { get; set; }  // decimal(9,6)

        public DateTime CreatedAt { get; set; }

        public ICollection<UserAddress> Users { get; set; } = new List<UserAddress>();

        public ICollection<VendorListing> VendorListings { get; set; } = new List<VendorListing>();

        public ICollection<Order> OrdersDeliverTo { get; set; } = new List<Order>();

        public ICollection<Shipment> ShipmentsPickup { get; set; } = new List<Shipment>();
        public ICollection<Shipment> ShipmentsDrop { get; set; } = new List<Shipment>();

        public ICollection<TransportRoute> RoutesFrom { get; set; } = new List<TransportRoute>();
        public ICollection<TransportRoute> RoutesTo { get; set; } = new List<TransportRoute>();
    }

    public class UserAddress
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }

        public string? Label { get; set; } // Home/Warehouse/etc
        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Address Address { get; set; } = null!;
    }
}
