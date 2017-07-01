using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace FlightAvailability.Model 
{
    public interface IImportFlights 
    {
        void Import();
    }

    public class ImportFlights : IImportFlights
    {
        private FlightContext _ctx;
       
        public ImportFlights(FlightContext ctx) 
        {
            this._ctx = ctx;
        }

        async void IImportFlights.Import()
        {
             InitialFlights().ForEach(f => _ctx.Flight.Add(f));
            await _ctx.SaveChangesAsync();
        }

        private List<Flight> InitialFlights() 
        {
            var flights = new List<Flight>
            {
                new Flight{Origin="MAD",Destination="GTW"},
                new Flight{Origin="MAD",Destination="FRA"},
                new Flight{Origin="MAD",Destination="LHR"},
                new Flight{Origin="MAD",Destination="ACE"}
            };
            return flights;
        }
    }
}