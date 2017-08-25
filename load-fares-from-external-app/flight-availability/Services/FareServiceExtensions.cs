
using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightAvailability.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FlightAvailability.Services
{
    public static class FareServiceExtensions 
    {
        public static IServiceCollection AddFareService(this IServiceCollection services, IConfigurationRoot config) 
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            services.AddOptions();
            services.Configure<FareServiceOptions>(config.GetSection("fare_service"));
            services.AddSingleton<IFareService, FareService>();

            return services;
        }
    }
}
