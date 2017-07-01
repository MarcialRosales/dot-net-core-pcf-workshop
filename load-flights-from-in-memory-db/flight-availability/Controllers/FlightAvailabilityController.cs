
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using FlightAvailability.Services;
using FlightAvailability.Model;

namespace FlightAvailability.Controllers
{

    [Route("api/")]
    public class FlightAvailabilityController : Controller
    {

        private IFlightService _flightService;

        public FlightAvailabilityController(IFlightService flightService)
        {
            this._flightService = flightService;
        }

        [HttpGet()]
        public async Task<List<Flight>> find([FromQuery, Required] string origin, [FromQuery, Required] string destination)
        {
            return await _flightService.find(origin, destination);

        }
    }
   

}

