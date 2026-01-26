using AutoMapper;
using VendorProject.Common.DTOs;
using VendorProject.EF.Models;

namespace VendorProject.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mapping
            CreateMap<Product, ProductDto>();

            // VendorListing mapping
            CreateMap<VendorListing, VendorListingDto>();

            // User mapping
            CreateMap<User, UserDto>();

            // TransportRoute mapping
            CreateMap<TransportRoute, TransportRouteDto>()
                .ForMember(dest => dest.PriceModel, opt => opt.MapFrom(src => src.PriceModel.ToString()));
        }
    }
}
