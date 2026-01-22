namespace VendorProject.EF.Models
{
    public class Vehicle
    {
        public Guid Id { get; set; }

        public Guid TransporterUserId { get; set; }

        public string VehicleType { get; set; } = null!; // Bike/Van/Truck
        public string? RegNumber { get; set; }

        public decimal? MaxVolume { get; set; } // decimal(18,3) m3
        public decimal? MaxWeight { get; set; } // decimal(18,3) kg

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public User TransporterUser { get; set; } = null!;
        public ICollection<TransportRoute> Routes { get; set; } = new List<TransportRoute>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }

    public class TransportRoute
    {
        public Guid Id { get; set; }

        public Guid TransporterUserId { get; set; }
        public Guid? VehicleId { get; set; }

        public Guid? FromAddressId { get; set; }
        public Guid? ToAddressId { get; set; }

        public string? FromCity { get; set; }
        public string? ToCity { get; set; }

        public DateOnly? DepartDate { get; set; }

        public decimal? CapacityWeightAvailable { get; set; } // decimal(18,3)
        public decimal? CapacityVolumeAvailable { get; set; } // decimal(18,3)

        public decimal? BasePrice { get; set; } // decimal(18,2)
        public PriceModel PriceModel { get; set; } = PriceModel.Flat;
        public string Currency { get; set; } = "INR";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public User TransporterUser { get; set; } = null!;
        public Vehicle? Vehicle { get; set; }
        public Address? FromAddress { get; set; }
        public Address? ToAddress { get; set; }

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
