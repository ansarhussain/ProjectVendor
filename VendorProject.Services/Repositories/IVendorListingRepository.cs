using VendorProject.Common.DTOs;
using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public interface IVendorListingRepository
    {
        Task<IEnumerable<VendorListing>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<VendorListing> Items, int TotalCount)> GetByProductIdPaginatedAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default);
        Task<VendorListing?> GetByProductIdAndVendorListingIdAsync(Guid productId, Guid vendorListingId, CancellationToken cancellationToken = default);
    }
}
