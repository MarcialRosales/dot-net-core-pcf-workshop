using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace fare_service.Controllers
{
    [Route("/")]
    public class FareController : Controller
    {
        Random r = new Random();

        // POST /
        [HttpPost]
        public List<string> applyFares([FromBody]List<Flight> flights)
        {
            Console.Write($"Applying fares to {flights.Count} flights");
            return flights.Select(flight => System.Convert.ToString(r.NextDouble())).ToList();
        }

        
    }

    public class Flight 
    {
        public int Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; } 
    }
}
