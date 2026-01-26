using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public interface IVendorListingRepository
    {
        Task<IEnumerable<VendorListing>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<VendorListing?> GetByProductIdAndVendorListingIdAsync(Guid productId, Guid vendorListingId, CancellationToken cancellationToken = default);
    }
}
