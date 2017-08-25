
using FlightAvailability.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector.App;
using Microsoft.Extensions.Options;

namespace FlightAvailability.Controllers
{
    [Route("mgt/")]
    public class ActuatorController : Controller
    {
        ILogger<ActuatorController> _logger;
        CloudFoundryApplicationOptions _appInfo;

        public ActuatorController(ILogger<ActuatorController> logger, IOptions<CloudFoundryApplicationOptions> appInfo)
        {
            _logger = logger;
            _appInfo = appInfo.Value;
        }


        // GET: mgt/info
        [HttpGet("info")]
        public CloudFoundryApplicationOptions info()
        {
            _logger?.LogDebug("requesting info");
            
           return _appInfo;
        }

    }
}
