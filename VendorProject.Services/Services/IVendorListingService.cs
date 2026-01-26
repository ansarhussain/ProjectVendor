using VendorProject.Common.DTOs;

namespace VendorProject.Services.Services
{
    public interface IVendorListingService
    {
        Task<IEnumerable<VendorListingDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<PaginatedResponse<VendorListingDto>> GetByProductIdPaginatedAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default);
        Task<VendorListingDto?> GetByProductIdAndVendorListingIdAsync(Guid productId, Guid vendorListingId, CancellationToken cancellationToken = default);
    }
}
