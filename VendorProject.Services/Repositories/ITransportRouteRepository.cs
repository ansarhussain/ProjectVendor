using VendorProject.Common.DTOs;
using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public interface ITransportRouteRepository
    {
        Task<IEnumerable<TransportRoute>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(IEnumerable<TransportRoute> Items, int TotalCount)> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    }
}
