using Microsoft.EntityFrameworkCore;
using VendorProject.EF.Data;
using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public class VendorListingRepository : IVendorListingRepository
    {
        private readonly MarketplaceDbContext _context;

        public VendorListingRepository(MarketplaceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VendorListing>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.VendorListings
                .AsNoTracking()
                .Where(vl => vl.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<VendorListing?> GetByProductIdAndVendorListingIdAsync(Guid productId, Guid vendorListingId, CancellationToken cancellationToken = default)
        {
            return await _context.VendorListings
                .AsNoTracking()
                .FirstOrDefaultAsync(vl => vl.ProductId == productId && vl.Id == vendorListingId, cancellationToken);
        }
    }
}
