
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightAvailability.Model;

namespace FlightAvailability.Services
{

    public class FlightService : IFlightService
    {
        private IFlightRepository _repo;

        public FlightService(IFlightRepository repo)
        {
            this._repo = repo;
        }


        async Task<List<Flight>> IFlightService.find(string origin, string destination)
        {
            return await _repo.findByOriginAndDestination(origin, destination);
        }
    }
}
