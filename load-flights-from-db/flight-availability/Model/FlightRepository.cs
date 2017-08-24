using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FlightAvailability.Model
{
    public class FlightRepository : IFlightRepository
    {
        private FlightContext _ctx;
        Random _random = new Random();

        public FlightRepository(FlightContext ctx)
        {
            _ctx = ctx;
        }

        async Task<List<Flight>> IFlightRepository.findByOriginAndDestination(string origin, string destination)
        {
            return await _ctx.Flights
                .Where(f => f.Origin == origin && f.Destination == destination)
                .ToListAsync();
        }
    }
}
