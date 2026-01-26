using Microsoft.AspNetCore.Mvc;
using VendorProject.Common.DTOs;
using VendorProject.Services.Services;

namespace VendorProject.Controllers
{
    /// <summary>
    /// Controller for managing products
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Gets all products with pagination and search
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <param name="searchName">Search term to filter by product name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of products</returns>
        /// <response code="200">Returns the paginated list of products</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchName = null,
            CancellationToken cancellationToken = default)
        {
            var query = new PaginationQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchName = searchName
            };

            var result = await _productService.GetAllPaginatedAsync(query, cancellationToken);
            return Ok(result);
        }
    }
}
