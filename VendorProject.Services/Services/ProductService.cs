using AutoMapper;
using VendorProject.Common.DTOs;
using VendorProject.Services.Repositories;

namespace VendorProject.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var products = await _repository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<PaginatedResponse<ProductDto>> GetAllPaginatedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var (products, totalCount) = await _repository.GetAllPaginatedAsync(query, cancellationToken);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            return new PaginatedResponse<ProductDto>
            {
                Data = productDtos,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
