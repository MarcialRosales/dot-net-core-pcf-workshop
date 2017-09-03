using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace _2_sample_dotnet_core.Controllers
{
    [Route("mgt")]
    public class ActuatorController : Controller
    {
        

        // GET info
        [HttpGet]
        [Route("info")]
        public AppInfo Get()
        {
            return new AppInfo();
        }


    }
    public class AppInfo
    {
     
    }
}
