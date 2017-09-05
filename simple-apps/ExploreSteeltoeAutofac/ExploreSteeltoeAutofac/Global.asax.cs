using Autofac;

using Autofac.Integration.WebApi;
using Autofac.Extensions.DependencyInjection;
using ExploreSteeltoeAutofac.Controllers;
using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using PA = Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Options;
using NLog;
using Autofac.Core;

namespace ExploreSteeltoeAutofac
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        void Application_Error(object sender, EventArgs e)
        {
            Exception lastError = Server.GetLastError();
            logger.Error("Unhandled exception: {lastError} {stack}", lastError.Message, lastError.StackTrace);
        }

        protected void Application_Start()
        {
            logger.Info("Starting application ...");

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<LogRequestModule>();

            // Register all the controllers with Autofac
            // By default types that implement IHttpController and have a name with the suffix Controller will be registered.
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var envName = "development";

            IServiceCollection services = new ServiceCollection();

            IConfigurationRoot Configuration = buildConfiguration(envName);
            services.AddOptions();
            services.Configure<CloudFoundryApplicationOptions>(Configuration);
            services.Configure<CloudFoundryServicesOptions>(Configuration);

            containerBuilder.Populate(services);
     
            containerBuilder.RegisterInstance(Configuration).As<IConfigurationRoot>();
            
            // Create the Autofac container
            IContainer container = containerBuilder.Build();
            

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            logger.Info("Started application ");
        }


        private IConfigurationRoot buildConfiguration(string envName)
        {
            var ContentRootPath = PA.PlatformServices.Default.Application.ApplicationBasePath;

            var configBuilder = new ConfigurationBuilder()
               .SetBasePath(ContentRootPath)
               //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
               //    .AddJsonFile($"appsettings.{envName}.json", optional: true)
               .AddEnvironmentVariables()
               .AddCloudFoundry();

            IConfigurationRoot Configuration = configBuilder.Build();

            return Configuration;

        }
    }
    public class LogRequestModule : Autofac.Module
    {
        public int depth = 0;

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry,
                                                              IComponentRegistration registration)
        {
            registration.Preparing += RegistrationOnPreparing;
            registration.Activating += RegistrationOnActivating;
            base.AttachToComponentRegistration(componentRegistry, registration);
        }

        private string GetPrefix()
        {
            return new string('-', depth * 2);
        }

        private void RegistrationOnPreparing(object sender, PreparingEventArgs preparingEventArgs)
        {
            Console.WriteLine("{0}Resolving  {1}", GetPrefix(), preparingEventArgs.Component.Activator.LimitType);
            depth++;
        }

        private void RegistrationOnActivating(object sender, ActivatingEventArgs<object> activatingEventArgs)
        {
            depth--;
            Console.WriteLine("{0}Activating {1}", GetPrefix(), activatingEventArgs.Component.Activator.LimitType);
        }
    }

}
