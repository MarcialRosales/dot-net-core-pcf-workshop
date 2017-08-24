using Microsoft.EntityFrameworkCore;


namespace FlightAvailability.Model
{
    public class FortuneContext : DbContext
    {
        public FortuneContext(DbContextOptions<FortuneContext> options) :
            base(options)
        {

        }
        public DbSet<FortuneEntity> Fortunes { get; set; }
    }
}
