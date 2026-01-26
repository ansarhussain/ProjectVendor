using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public interface ITransportRouteRepository
    {
        Task<IEnumerable<TransportRoute>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
