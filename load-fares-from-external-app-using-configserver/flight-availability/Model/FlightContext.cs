using Microsoft.EntityFrameworkCore;


namespace FlightAvailability.Model
{
    public class FlightContext : DbContext
    {
        public FlightContext(DbContextOptions<FlightContext> options) :
            base(options)
        {

        }
        public DbSet<Flight> Flights { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flight>()
            .ToTable("flights");
    }
    }
}
