using Microsoft.EntityFrameworkCore;
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
    }
}
