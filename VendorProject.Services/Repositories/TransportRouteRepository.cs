using Microsoft.EntityFrameworkCore;
using VendorProject.Common.DTOs;
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

        public async Task<(IEnumerable<TransportRoute> Items, int TotalCount)> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var queryable = _context.TransportRoutes.AsNoTracking();

            // Apply search filter by FromCity or ToCity
            if (!string.IsNullOrWhiteSpace(query.SearchName))
            {
                queryable = queryable.Where(tr => 
                    (tr.FromCity != null && tr.FromCity.Contains(query.SearchName)) ||
                    (tr.ToCity != null && tr.ToCity.Contains(query.SearchName)));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .OrderBy(tr => tr.FromCity)
                .ThenBy(tr => tr.ToCity)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
