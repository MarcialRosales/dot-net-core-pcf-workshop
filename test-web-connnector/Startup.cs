using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.Services;

namespace test_web_connnector
{
    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }
        
        public Startup(IHostingEnvironment env)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                .AddCloudFoundry();

            Configuration = builder.Build();
            
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging()
                    .AddCloudFoundry(Configuration)
                    .AddSingleton<IFooService, FooService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(
                    context.RequestServices.GetRequiredService<IFooService>().DoThing());
            });
        }
    }
    public interface IFooService
    {
        string DoThing();
    }
    public class FooService : IFooService
    {
        private readonly ILogger<FooService> _logger;
        private readonly CloudFoundryApplicationOptions _appInfo;
        private readonly CloudFoundryServicesOptions _serviceInfo;

        private readonly IConfigurationRoot _config;
        public FooService(ILoggerFactory loggerFactory, IConfigurationRoot config, IOptions<CloudFoundryApplicationOptions> appInfo,
            IOptions<CloudFoundryServicesOptions> serviceInfo)
        {
            _logger = loggerFactory.CreateLogger<FooService>();
            _appInfo = appInfo.Value;
            _serviceInfo = serviceInfo.Value;
            _config = config;
        }

        public string DoThing()
        {
             StringBuilder sb = new StringBuilder($"App.name: {_appInfo.ApplicationName} App.index: {_appInfo.InstanceIndex}\n");
             foreach(Service service in _serviceInfo.Services)
             {
                 sb.Append($"Service name: {service.Name} type: {service.Label} ({service.GetType()}) \n");
             }
             sb.Append(LookupMySql());
             sb.Append(LookupOracle());
             
             return sb.ToString();
        }

        public string LookupMySql() 
        {
            MySqlServiceInfo info = _config.GetSingletonServiceInfo<MySqlServiceInfo>();
            return $"MySql database  host:{info.Host} port:{info.Port} user:{info.UserName} schema:{info.Path} \n ";
        }
        public string LookupOracle() 
        {
            OracleServiceInfo info = _config.GetSingletonServiceInfo<OracleServiceInfo>();
            return $"Oracle database  host:{info.Host} port:{info.Port} user:{info.UserName} schema:{info.Path} \n ";
        }
    }
}
