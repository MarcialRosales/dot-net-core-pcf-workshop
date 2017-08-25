
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using FlightAvailability.Services;
using FlightAvailability.Model;
using Microsoft.Extensions.Logging;

namespace FlightAvailability.Controllers
{

    [Route("api/")]
    public class FlightAvailabilityController : Controller
    {

        private IFlightService _flightService;
        private ILogger<FlightAvailabilityController> _logger;

        public FlightAvailabilityController(ILogger<FlightAvailabilityController> logger, IFlightService flightService)
        {
            this._flightService = flightService;
            this._logger = logger;

            _logger.LogDebug("Created FlightAvailabilityController");
        }

        [HttpGet()]
        public async Task<IEnumerable<Flight>> find([FromQuery, Required] string origin, [FromQuery, Required] string destination)
        {
            return await _flightService.find(origin, destination);

        }
        [HttpGet("fares")]
        public async Task<IEnumerable<FaredFlight>> findWithFares([FromQuery, Required] string origin, [FromQuery, Required] string destination)
        {
            return await _flightService.findWithFares(origin, destination);

        }
    }
   

}

