
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FlightAvailability.Model;

namespace FlightAvailability.Services
{

    public class FlightService : IFlightService
    {
        private IFlightRepository _repo;
        private IFareService _fareService;

        public FlightService(IFlightRepository repo, IFareService fareService)
        {
            this._repo = repo;
            this._fareService = fareService;
        }


        async Task<List<Flight>> IFlightService.find(string origin, string destination)
        {
            return await _repo.findByOriginAndDestination(origin, destination);
        }
        private  List<Flight> InitialFlights() 
        {
            var flights = new List<Flight>
            {
                new Flight{Id=1, Origin="MAD",Destination="GTW"},
                new Flight{Id=2, Origin="MAD",Destination="FRA"},
                new Flight{Id=3, Origin="MAD",Destination="LHR"},
                new Flight{Id=4, Origin="MAD",Destination="ACE"}
            };
            return flights;
        }
        async Task<List<FaredFlight>> IFlightService.findWithFares(string origin, string destination)
        {
             List<Flight> flights = await _repo.findByOriginAndDestination(origin, destination);
             if (flights.Count < 1) 
                return new List<FaredFlight>(); 
            //List<Flight> flights = InitialFlights();

            Console.Write($"Retrived {flights.Count} flights");

            List<string> fares = await _fareService.applyFares(flights);
            return Enumerable.Range(1, flights.Count).
                Select(i =>  new FaredFlight(flights[i-1], fares[i-1]) ).ToList();
        }
        
    }
}
