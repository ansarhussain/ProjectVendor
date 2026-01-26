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
        /// Gets all transport routes
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of transport routes</returns>
        /// <response code="200">Returns the list of transport routes</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TransportRouteDto>>> GetTransports(CancellationToken cancellationToken)
        {
            var transportRoutes = await _transportRouteService.GetAllAsync(cancellationToken);
            return Ok(transportRoutes);
        }
    }
}
