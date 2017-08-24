using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FlightAvailability.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FlightAvailability.Model 
{
    public class ImportFlights2
    {

        public static void Import(FlightContext ctx)
        {
                ctx.Database.EnsureCreated();

                if (ctx.Flights.Count() == 0) {
                    InitialFlights().ForEach(f => ctx.Flights.Add(f));
                    ctx.SaveChanges();
                }
                    
        }

        private static List<Flight> InitialFlights() 
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
    }
}