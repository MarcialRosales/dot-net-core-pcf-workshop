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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Swashbuckle.AspNetCore.Swagger;

using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;

using FlightAvailability.Services;
using FlightAvailability.Model;

namespace FlightAvailability
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
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
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine($"App name : {Configuration["vcap:application:name"]}");

            services.AddOptions();
            services.AddCloudFoundry(Configuration);

            services.Configure<FareServiceOptions>(Configuration.GetSection("fare-service"));

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
            
            services.AddSingleton<HttpClient>(buildHttpClient(Configuration.GetSection("fare_service")));
            
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

        private HttpClient buildHttpClient(IConfiguration config) {
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(config.GetValue<String>("url"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Console.Write($"HTTP client {client.BaseAddress}");
            return client;
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, FlightContext ctx)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();


            FlightAvailability.Model.ImportFlights2.Import(ctx);
            //app.ApplicationServices.GetRequiredService<IImportFlights>().Import();
            //FlightAvailability.Model.SampleData.InitializeFortunesAsync(app.ApplicationServices).Wait();
        }
    }

    public class FareServiceOptions 
    {
        public FareServiceOptions() 
        {

        }
        public string url { get; set; }

    }

}
