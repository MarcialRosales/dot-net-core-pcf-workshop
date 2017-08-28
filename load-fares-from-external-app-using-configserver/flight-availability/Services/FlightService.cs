
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FlightAvailability.Model;
using Microsoft.Extensions.Logging;

namespace FlightAvailability.Services
{

    public class FlightService : IFlightService
    {
        private IFlightRepository _repo;
        private IFareService _fareService;
        private ILogger<FlightService> _logger;

        public FlightService(IFlightRepository repo, IFareService fareService, ILogger<FlightService> logger)
        {
            this._repo = repo;
            this._fareService = fareService;
            this._logger = logger;
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

            _logger.LogInformation(LoggingEvents.RequestFlightWithFares, "Pricing {flightCount} flights for {origin}/{destination} at {RequestTime}", 
                flights.Count, origin, destination, DateTime.Now);

            List<string> fares = await _fareService.applyFares(flights);
            return Enumerable.Range(1, flights.Count).
                Select(i =>  new FaredFlight(flights[i-1], fares[i-1]) ).ToList();
        }
        
    }
    class LoggingEvents 
    {
        public const int RequestFlightWithFares = 1000;
    }
}
