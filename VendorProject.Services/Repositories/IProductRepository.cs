using VendorProject.Common.DTOs;
using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    }
}
