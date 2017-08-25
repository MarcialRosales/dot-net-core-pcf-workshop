using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Swashbuckle.AspNetCore.Swagger;

using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Pivotal.Discovery.Client;

using FlightAvailability.Services;
using FlightAvailability.Model;

namespace FlightAvailability
{
    public class Startup
    {
        
        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Console.Write("using environment: "+ env.EnvironmentName);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddCloudFoundry()
                .AddEnvironmentVariables();
                
            Configuration = builder.Build();
            Environment = env;

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();


        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine($"App name : {Configuration["vcap:application:name"]}");

            services.AddOptions();
            services.AddCloudFoundry(Configuration);
            services.AddDiscoveryClient(Configuration);

            if (Environment.IsDevelopment())
            {
                services.AddEntityFramework()
                    .AddDbContext<FlightContext>(options => options.UseInMemoryDatabase());
                services.AddEntityFramework()
                    .AddDbContext<FortuneContext>(options => options.UseInMemoryDatabase());

            } else
            {
                services.AddEntityFramework()
                     .AddDbContext<FlightContext>(options => options.UseMySql(Configuration));
                services.AddEntityFramework()
                    .AddDbContext<FortuneContext>(options => options.UseMySql(Configuration));
                
            }
            
            services.Configure<HttpClientOptions>(Configuration.GetSection("fare_service"));
            services.AddSingleton<HttpClientProvider>();
            
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<IFlightRepository, FlightRepository>();
            services.AddSingleton<IFortuneRepository, FortuneRepository>();
            services.AddSingleton<IFareService, FareService>();
            services.AddSingleton<IFlightService, FlightService>();
            services.AddSingleton<IImportFlights, ImportFlights>();
            
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Flight Availability API", Version = "v1" });
            });
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, FlightContext ctx)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.UseDiscoveryClient();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();


            FlightAvailability.Model.ImportFlights2.Import(ctx);
            //app.ApplicationServices.GetRequiredService<IImportFlights>().Import();
            //FlightAvailability.Model.SampleData.InitializeFortunesAsync(app.ApplicationServices).Wait();
        }
    }

    public class HttpClientOptions 
    {
        public HttpClientOptions()
        {

        }
        public string Url { get; set; }

    }

    // one HttpClient for each each endpoint (https://docs.microsoft.com/en-gb/dotnet/api/system.net.http.httpclient?view=netcore-1.1)
    public class HttpClientProvider
    {
        public HttpClient client { get; private set;}
        
        private IDiscoveryClient _discoveryClient;
        private ILogger<HttpClientProvider> _logger;

        public HttpClientProvider(IDiscoveryClient discoveryClient, 
            IOptions<HttpClientOptions> options,
            ILogger<HttpClientProvider> logger) {
            
            this._logger = logger;

            _logger.LogDebug("Building HttpClientProvider");
            if (discoveryClient != null) 
                Console.Write(" with discoveryClient");
            if (options.Value != null) 
                Console.Write($" with options {options.Value}");
            this._discoveryClient = discoveryClient;

            _logger.LogDebug("******** services: ");
            foreach (var service in discoveryClient.Services) 
                _logger.LogDebug($"{service}, ");

            _logger.LogDebug("******** fare-service: ");
            foreach (var service in discoveryClient.GetInstances("fare-service")) 
                _logger.LogDebug($"{service.ServiceId}:{service.Uri}, ");

            _logger.LogDebug("******** FARE-SERVICE: ");
            foreach (var service in discoveryClient.GetInstances("FARE-SERVICE")) 
                _logger.LogDebug($"{service.ServiceId}:{service.Uri}, ");

            this.configure(options.Value);
        }

        private void configure(HttpClientOptions config) { 
            
            if (_discoveryClient != null) 
            {
                Console.Write($"Building httpClient with discoveryClient and url {config.Url}");
                client = new HttpClient(new DiscoveryHttpClientHandler(_discoveryClient), false);
            }else 
            {
                Console.Write($"Building httpClient and url {config.Url}");
                client = new HttpClient();
            }

            client.BaseAddress = new Uri(config.Url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Console.Write("Built httpClient");
        }

        
    }

}
