using System.Collections.Generic;
using System.Threading.Tasks;
using FlightAvailability.Model;

namespace FlightAvailability.Services
{
    public interface IFareService
    {
        Task<List<string>> applyFares(List<Flight> flights);

    }
}