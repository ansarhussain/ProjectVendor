using VendorProject.Common.DTOs;

namespace VendorProject.Services.Services
{
    public interface ITransportRouteService
    {
        Task<IEnumerable<TransportRouteDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PaginatedResponse<TransportRouteDto>> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    }
}
