using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightAvailability.Model
{
    public class FlightRepository : IFlightRepository
    {
        private FlightContext _ctx;
        private ILogger<FlightRepository> _logger;

        public FlightRepository(ILogger<FlightRepository> logger, FlightContext ctx)
        {
            _ctx = ctx;
            _logger = logger;

            logger.LogDebug("Created FlightRepository");
        }

        async Task<List<Flight>> IFlightRepository.findByOriginAndDestination(string origin, string destination)
        {
                

                return await _ctx.Flights
                    .Where(f => f.Origin == origin && f.Destination == destination)
                    .ToListAsync();
        }
    }
}
