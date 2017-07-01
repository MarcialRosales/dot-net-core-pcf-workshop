using System.Collections.Generic;
using System.Threading.Tasks;
using FlightAvailability.Model;

namespace FlightAvailability.Services
{
    public interface IFlightService
    {
        Task<List<Flight>> find(string origin, string destination);

    }
}