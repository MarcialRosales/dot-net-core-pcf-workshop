using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAvailability.Model
{
    [Table("flights")]
    public class Flight {
        public int Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }

        public Flight() 
        {

        }
        public Flight(Flight flight) 
        {
            Id = flight.Id;
            Origin = flight.Origin;
            Destination = flight.Destination;
        }
        public override string ToString()
        {
            return $"Flight[{Id} {Origin}/{Destination}]";
        }
    }

    
}