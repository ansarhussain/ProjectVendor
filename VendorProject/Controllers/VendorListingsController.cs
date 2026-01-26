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
        /// Gets all vendor listings for a specific product
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of vendor listings for the product</returns>
        /// <response code="200">Returns the list of vendor listings</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VendorListingDto>>> GetVendorListingsByProductId(
            Guid productId,
            CancellationToken cancellationToken)
        {
            var vendorListings = await _vendorListingService.GetByProductIdAsync(productId, cancellationToken);
            return Ok(vendorListings);
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
