using VendorProject.Common.DTOs;

namespace VendorProject.Services.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
