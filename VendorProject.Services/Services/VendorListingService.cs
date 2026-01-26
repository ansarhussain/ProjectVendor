using AutoMapper;
using VendorProject.Common.DTOs;
using VendorProject.Services.Repositories;

namespace VendorProject.Services.Services
{
    public class VendorListingService : IVendorListingService
    {
        private readonly IVendorListingRepository _repository;
        private readonly IMapper _mapper;

        public VendorListingService(IVendorListingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VendorListingDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var vendorListings = await _repository.GetByProductIdAsync(productId, cancellationToken);
            return _mapper.Map<IEnumerable<VendorListingDto>>(vendorListings);
        }

        public async Task<VendorListingDto?> GetByProductIdAndVendorListingIdAsync(Guid productId, Guid vendorListingId, CancellationToken cancellationToken = default)
        {
            var vendorListing = await _repository.GetByProductIdAndVendorListingIdAsync(productId, vendorListingId, cancellationToken);
            return vendorListing == null ? null : _mapper.Map<VendorListingDto>(vendorListing);
        }
    }
}
