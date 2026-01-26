using VendorProject.Common.DTOs;

namespace VendorProject.Services.Services
{
    public interface ITransportRouteService
    {
        Task<IEnumerable<TransportRouteDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
