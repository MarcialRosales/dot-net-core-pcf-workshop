using Steeltoe.CloudFoundry.Connector.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ExploreSteeltoeAutofac.Controllers
{
    [Route("faulty")]
    public class FaultyController : ApiController
    {
        public FaultyController(OracleServiceInfo oracleDbInfo)
        {

        }

        [HttpGet]
        public string Info()
        {
            return "{}";  
            
        }
    }
}
