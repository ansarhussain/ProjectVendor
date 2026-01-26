using VendorProject.EF.Models;

namespace VendorProject.Services.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
