using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.CloudFoundry.Connector;

namespace ExploreSteeltoeAutofac.Controllers
{
    [Route("test")]
    public class TestController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private AppInfo info;

        public TestController(IConfigurationRoot config, IOptions<CloudFoundryServicesOptions> opsServInfo)
        {
            config.Bind(_cfApp);
            var env = config.GetSection("ASPNETCORE_ENVIRONMENT").Value;
            logger.Debug($"Created TestController with {env}");
            logger.Debug($"CloudFoundryApplicationOptions {_cfApp.ApplicationId} {_cfApp.ApplicationName} {_cfApp.InstanceIndex}");
            _cfServices = opsServInfo.Value;

            OracleServiceInfo oracleConnOpts = config.GetServiceInfo< OracleServiceInfo>("oracle-db");

            this.info = new AppInfo(config["vcap:application:name"], oracleConnOpts);
        }
        private CloudFoundryApplicationOptions _cfApp = new CloudFoundryApplicationOptions();
        private CloudFoundryServicesOptions _cfServices = new CloudFoundryServicesOptions();

        [HttpGet]
        public CloudFoundryApplicationOptions Info()
        {
            logger.Debug("ActuatorController.Info");
            return _cfApp;
        }
        [HttpGet]
        [Route("services")]
        public CloudFoundryServicesOptions Services()
        {
            logger.Debug("ActuatorController.Services");
            return _cfServices;
        }
        [HttpGet]
        [Route("info")]
        public AppInfo AppInfo()
        {
            logger.Debug("ActuatorController.Info");
            return info;
        }
    }
    public class AppInfo
    {
        public string Name { get; set; }
        public OracleServiceInfo Oracle  { get; set; }

        public AppInfo(string name, OracleServiceInfo oracle)
        {
            this.Name = name;
            this.Oracle = oracle;
        }
    }
}
