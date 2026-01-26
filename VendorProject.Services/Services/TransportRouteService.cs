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

        public async Task<PaginatedResponse<TransportRouteDto>> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var (transportRoutes, totalCount) = await _repository.GetAllPaginatedAsync(query, cancellationToken);
            var transportRouteDtos = _mapper.Map<IEnumerable<TransportRouteDto>>(transportRoutes);

            return new PaginatedResponse<TransportRouteDto>
            {
                Data = transportRouteDtos,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
