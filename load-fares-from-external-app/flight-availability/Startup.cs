using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;

using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;

using FlightAvailability.Services;
using FlightAvailability.Model;
using NLog.Extensions.Logging;
using NLog.Web;
using Newtonsoft.Json;

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

            env.ConfigureNLog("nlog.config");
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCloudFoundry(Configuration);

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
            
            services.AddFareService(Configuration);
            
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<IFlightRepository, FlightRepository>();
            services.AddSingleton<IFortuneRepository, FortuneRepository>();
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
//            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
//            loggerFactory.AddDebug();
            
            // add NLog to ASP.NET Core instead of default Console ILogger
            loggerFactory.AddNLog();
            app.AddNLogWeb();

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            FlightAvailability.Model.ImportFlights2.Import(ctx);
            //app.ApplicationServices.GetRequiredService<IImportFlights>().Import();
            //FlightAvailability.Model.SampleData.InitializeFortunesAsync(app.ApplicationServices).Wait();
        }
    }
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger<Startup> _logger;
        private Dictionary<string, HttpStatusCode> handlers = new Dictionary<string, HttpStatusCode>();

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<Startup> logger)
        {
            this.next = next;
            this._logger = logger;

            handlers[typeof(System.Net.Http.HttpRequestException).Name] = HttpStatusCode.BadRequest;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, $"{ex.GetType().Name} : {ex.Message}");

                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode code = handlers.ContainsKey(exception.GetType().Name) ? 
                handlers[exception.GetType().Name] : HttpStatusCode.InternalServerError;

            var result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }

    
    }
    

}
