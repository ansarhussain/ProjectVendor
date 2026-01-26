using Microsoft.AspNetCore.Mvc;
using VendorProject.Common.DTOs;
using VendorProject.Services.Services;

namespace VendorProject.Controllers
{
    /// <summary>
    /// Controller for managing vendor listings
    /// </summary>
    [ApiController]
    [Route("api/products/{productId}/vendor-listings")]
    public class VendorListingsController : ControllerBase
    {
        private readonly IVendorListingService _vendorListingService;

        public VendorListingsController(IVendorListingService vendorListingService)
        {
            _vendorListingService = vendorListingService;
        }

        /// <summary>
        /// Gets all vendor listings for a specific product with pagination and search
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <param name="searchName">Search term to filter by product name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of vendor listings for the product</returns>
        /// <response code="200">Returns the paginated list of vendor listings</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<VendorListingDto>>> GetVendorListingsByProductId(
            Guid productId,
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

            var result = await _vendorListingService.GetByProductIdPaginatedAsync(productId, query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific vendor listing by product ID and vendor listing ID
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="vendorListingId">The vendor listing ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The vendor listing details</returns>
        /// <response code="200">Returns the vendor listing</response>
        /// <response code="404">Vendor listing not found</response>
        [HttpGet("{vendorListingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VendorListingDto>> GetVendorListingById(
            Guid productId,
            Guid vendorListingId,
            CancellationToken cancellationToken)
        {
            var vendorListing = await _vendorListingService.GetByProductIdAndVendorListingIdAsync(
                productId,
                vendorListingId,
                cancellationToken);

            if (vendorListing == null)
            {
                return NotFound();
            }

            return Ok(vendorListing);
        }
    }
}
