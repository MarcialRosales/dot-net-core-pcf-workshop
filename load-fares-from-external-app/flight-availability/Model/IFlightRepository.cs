using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightAvailability.Model {
    public interface IFlightRepository 
    {
        Task<List<Flight>> findByOriginAndDestination(string origin, string destination);
    }
}