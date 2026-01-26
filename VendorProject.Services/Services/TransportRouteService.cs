using AutoMapper;
using VendorProject.Common.DTOs;
using VendorProject.Services.Repositories;

namespace VendorProject.Services.Services
{
    public class TransportRouteService : ITransportRouteService
    {
        private readonly ITransportRouteRepository _repository;
        private readonly IMapper _mapper;

        public TransportRouteService(ITransportRouteRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransportRouteDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var transportRoutes = await _repository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<TransportRouteDto>>(transportRoutes);
        }
    }
}
