namespace VendorProject.Common.DTOs
{
    /// <summary>
    /// VendorListing read DTO
    /// </summary>
    public class VendorListingDto
    {
        public Guid Id { get; set; }
        public Guid VendorUserId { get; set; }
        public Guid ProductId { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal MinOrderQty { get; set; }
        public decimal PricePerUnit { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime? PriceValidFrom { get; set; }
        public DateTime? PriceValidTo { get; set; }
        public Guid? ListingAddressId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
