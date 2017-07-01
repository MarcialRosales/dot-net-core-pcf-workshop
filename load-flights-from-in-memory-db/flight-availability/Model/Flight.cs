using System;

namespace FlightAvailability.Model
{
    public class Flight {
        public long Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }

        public override string ToString()
        {
            return $"Flight[{Id} {Origin}/{Destination}]";
        }
    }

}