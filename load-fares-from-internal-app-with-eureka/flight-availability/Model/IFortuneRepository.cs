using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightAvailability.Model
{
    public interface IFortuneRepository
    {
        Task<List<FortuneEntity>> GetAllAsync();

        Task<FortuneEntity> RandomFortuneAsync();
    }
}
