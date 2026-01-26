using Microsoft.EntityFrameworkCore;
using VendorProject.Common.DTOs;
using VendorProject.EF.Data;
using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MarketplaceDbContext _context;

        public UserRepository(MarketplaceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var queryable = _context.Users.AsNoTracking();

            // Apply search filter by FullName
            if (!string.IsNullOrWhiteSpace(query.SearchName))
            {
                queryable = queryable.Where(u => u.FullName.Contains(query.SearchName));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .OrderBy(u => u.FullName)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
    }
}
