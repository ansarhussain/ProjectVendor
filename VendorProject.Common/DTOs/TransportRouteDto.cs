namespace VendorProject.Common.DTOs
{
    /// <summary>
    /// TransportRoute read DTO
    /// </summary>
    public class TransportRouteDto
    {
        public Guid Id { get; set; }
        public Guid TransporterUserId { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? FromAddressId { get; set; }
        public Guid? ToAddressId { get; set; }
        public string? FromCity { get; set; }
        public string? ToCity { get; set; }
        public DateOnly? DepartDate { get; set; }
        public decimal? CapacityWeightAvailable { get; set; }
        public decimal? CapacityVolumeAvailable { get; set; }
        public decimal? BasePrice { get; set; }
        public string PriceModel { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
