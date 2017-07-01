using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace test_connector
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCloudFoundry();
                
            Configuration = builder.Build();

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddCloudFoundry(Configuration)
                .AddSingleton<IFooService, FooService>()
                .BuildServiceProvider();
            

            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogDebug("Demonstrate how to retrieve app and service information from PCF");
            logger.LogDebug($"vcap application : {Configuration["VCAP_APPLICATION"]}");
            logger.LogDebug($"app name : {Configuration["vcap:application:name"]}");
            

            IConfigurationRoot config = serviceProvider.GetRequiredService<IConfigurationRoot>();
            logger.LogDebug($"app name (2) : {config["vcap:application:name"]}");
            
            //do the actual work here
            var foo = serviceProvider.GetService<IFooService>();
            foo.DoThing();

        }
    }

    public interface IFooService
    {
        void DoThing();
    }
    public class FooService : IFooService
    {
        private readonly ILogger<FooService> _logger;
        private readonly CloudFoundryApplicationOptions _appInfo;
        private readonly CloudFoundryServicesOptions _serviceInfo;

        public FooService(ILoggerFactory loggerFactory, IOptions<CloudFoundryApplicationOptions> appInfo,
            IOptions<CloudFoundryServicesOptions> serviceInfo)
        {
            _logger = loggerFactory.CreateLogger<FooService>();
            _appInfo = appInfo.Value;
            _serviceInfo = serviceInfo.Value;
        }

        public void DoThing()
        {
             _logger.LogInformation($"App.name: {_appInfo.ApplicationName} App.index: {_appInfo.InstanceIndex}");
             foreach(Service service in _serviceInfo.Services)
             {
                 _logger.LogInformation($"Service name: {service.Name} type: {service.Label} ");
             }
          
        }
    }
}
