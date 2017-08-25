using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAvailability.Model
{

    public class FaredFlight : Flight {
        
        public FaredFlight(Flight flight, string fare) : base(flight)
        {
            Fare = fare;
        }
        public string Fare { get; set; }
        
        public override string ToString()
        {
            return $"Flight[{Id} {Origin}/{Destination} @ {Fare}]";
        }
    }
}