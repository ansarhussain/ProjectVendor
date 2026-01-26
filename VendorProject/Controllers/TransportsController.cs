using Microsoft.AspNetCore.Mvc;
using VendorProject.Common.DTOs;
using VendorProject.Services.Services;

namespace VendorProject.Controllers
{
    /// <summary>
    /// Controller for managing transport routes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransportsController : ControllerBase
    {
        private readonly ITransportRouteService _transportRouteService;

        public TransportsController(ITransportRouteService transportRouteService)
        {
            _transportRouteService = transportRouteService;
        }

        /// <summary>
        /// Gets all transport routes with pagination and search
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <param name="searchName">Search term to filter by from city or to city</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of transport routes</returns>
        /// <response code="200">Returns the paginated list of transport routes</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<TransportRouteDto>>> GetTransports(
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

            var result = await _transportRouteService.GetAllPaginatedAsync(query, cancellationToken);
            return Ok(result);
        }
    }
}
