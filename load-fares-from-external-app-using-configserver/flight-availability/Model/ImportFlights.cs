using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FlightAvailability.Model 
{
    public interface IImportFlights 
    {
        void Import();
        
    }

    public class ImportFlights : IImportFlights
    {
        private IServiceProvider _serviceProvider;
        private ILogger<ImportFlights> _logger;
       
        public ImportFlights(ILogger<ImportFlights> logger, IServiceProvider serviceProvider) 
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }

        void IImportFlights.Import()
        {
            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<FlightContext>();

                _logger?.LogInformation("Ensuring database");
                db.Database.EnsureCreated();

                
            }

            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<FlightContext>();

                InitialFlights().ForEach(f => db.Add(f));
                db.SaveChanges();
                    
                _logger?.LogInformation("Added flights");
            }
         
        }

        private List<Flight> InitialFlights() 
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