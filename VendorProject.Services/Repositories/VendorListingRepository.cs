using Microsoft.EntityFrameworkCore;
using VendorProject.Common.DTOs;
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

        public async Task<(IEnumerable<VendorListing> Items, int TotalCount)> GetByProductIdPaginatedAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var queryable = _context.VendorListings
                .AsNoTracking()
                .Include(vl => vl.Product)
                .Where(vl => vl.ProductId == productId);

            // Apply search filter by Product.Name
            if (!string.IsNullOrWhiteSpace(query.SearchName))
            {
                queryable = queryable.Where(vl => vl.Product.Name.Contains(query.SearchName));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .OrderBy(vl => vl.Product.Name)
                .ThenBy(vl => vl.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<VendorListing?> GetByProductIdAndVendorListingIdAsync(Guid productId, Guid vendorListingId, CancellationToken cancellationToken = default)
        {
            return await _context.VendorListings
                .AsNoTracking()
                .FirstOrDefaultAsync(vl => vl.ProductId == productId && vl.Id == vendorListingId, cancellationToken);
        }
    }
}
