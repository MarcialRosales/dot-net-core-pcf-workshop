
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
using Microsoft.Extensions.Logging;

namespace FlightAvailability.Services
{

    public class FareService : IFareService
    {
        private HttpClient _client;
        private ILogger<FareService> _logger;

        public FareService(IOptions<FareServiceOptions> options, ILogger<FareService> logger)
        {
            this._logger = logger;
            this._client = buildHttpClient(options.Value);
        }

        private HttpClient buildHttpClient(FareServiceOptions config) {
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(config.Url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger.LogDebug($"Built HttpClient with url {config.Url}");

            return client;
        }

        async Task<List<string>> IFareService.applyFares(List<Flight> flights)
        {
            List<string> fares = null;

            var json = JsonConvert.SerializeObject(flights);               
            var content = new StringContent(json, Encoding.UTF8, "application/json");                

            HttpResponseMessage response = await _client.PostAsync("/", content);
            _logger.LogDebug($"Requesting fares for flights {json} thru client {_client.BaseAddress} go {response.StatusCode}");
            if (response.StatusCode == HttpStatusCode.OK) 
             {
                 json = await response.Content.ReadAsStringAsync(); 
                 fares = JsonConvert.DeserializeObject<List<string>>(json);

                 _logger.LogDebug($"Received {fares.Count} fares");
                 return fares;
             }else 
             {
                _logger.LogError("Failed to send http request");
                throw new HttpRequestException();
             }
            
        }
    }
    public class FareServiceOptions 
    {
        public FareServiceOptions() 
        {

        }
        public string Url { get; set; }

    }
   
}
