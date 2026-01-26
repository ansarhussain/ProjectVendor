using Microsoft.EntityFrameworkCore;
using VendorProject.EF.Data;
using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public class TransportRouteRepository : ITransportRouteRepository
    {
        private readonly MarketplaceDbContext _context;

        public TransportRouteRepository(MarketplaceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransportRoute>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.TransportRoutes
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
