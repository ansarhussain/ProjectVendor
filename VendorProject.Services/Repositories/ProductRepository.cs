using Microsoft.EntityFrameworkCore;
using VendorProject.Common.DTOs;
using VendorProject.EF.Data;
using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly MarketplaceDbContext _context;

        public ProductRepository(MarketplaceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var queryable = _context.Products.AsNoTracking();

            // Apply search filter by Name
            if (!string.IsNullOrWhiteSpace(query.SearchName))
            {
                queryable = queryable.Where(p => p.Name.Contains(query.SearchName));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .OrderBy(p => p.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
