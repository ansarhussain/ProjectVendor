namespace VendorProject.EF.Models
{
    public class Shipment
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Guid TransporterUserId { get; set; }

        public Guid? TransportRouteId { get; set; }
        public Guid? VehicleId { get; set; }

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Requested;

        public decimal? QuotedPrice { get; set; } // decimal(18,2)
        public decimal? FinalPrice { get; set; }  // decimal(18,2)
        public string Currency { get; set; } = "INR";

        public Guid PickupAddressId { get; set; }
        public Guid DropAddressId { get; set; }

        public DateTime? PickupTime { get; set; }
        public DateTime? DeliveredTime { get; set; }

        public DateTime CreatedAt { get; set; }

        public Order Order { get; set; } = null!;
        public User TransporterUser { get; set; } = null!;
        public TransportRoute? TransportRoute { get; set; }
        public Vehicle? Vehicle { get; set; }

        public Address PickupAddress { get; set; } = null!;
        public Address DropAddress { get; set; } = null!;

        public ICollection<ShipmentContact> ShipmentContacts { get; set; } = new List<ShipmentContact>();
    }

    public class ShipmentContact
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }

        public string ContactName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = "Driver"; // Driver/Helper etc.

        public bool IsSharedWithBuyer { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public Shipment Shipment { get; set; } = null!;
    }
}
