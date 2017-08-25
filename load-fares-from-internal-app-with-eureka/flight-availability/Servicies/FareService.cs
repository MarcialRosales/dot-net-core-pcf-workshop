
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightAvailability.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace FlightAvailability.Services
{

    public class FareService : IFareService
    {
        private HttpClient _client;
        
        public FareService(HttpClientProvider provider)
        {
            Console.Write("Buliding FareService");
            this._client = provider.client;        
        }


        async Task<List<string>> IFareService.applyFares(List<Flight> flights)
        {
            List<string> fares = null;

            var json = JsonConvert.SerializeObject(flights);               
            var content = new StringContent(json, Encoding.UTF8, "application/json");                

            HttpResponseMessage response = await _client.PostAsync("/", content);
            Console.Write($"Requesting fares for flights {json} thru client {_client.BaseAddress} go {response.StatusCode}");
            if (response.StatusCode == HttpStatusCode.OK) 
             {
                 json = await response.Content.ReadAsStringAsync(); 
                 fares = JsonConvert.DeserializeObject<List<string>>(json);

                 Console.Write($"Received {fares.Count} fares");
                 return fares;
             }else 
             {
                throw new HttpRequestException();
             }
            
        }
    }
}
