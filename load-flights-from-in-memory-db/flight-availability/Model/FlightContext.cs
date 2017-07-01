using Microsoft.EntityFrameworkCore;


namespace FlightAvailability.Model
{
    public class FlightContext : DbContext
    {
        public FlightContext(DbContextOptions<FlightContext> options) :
            base(options)
        {

        }
        public DbSet<Flight> Flight { get; set; }
    }
}
