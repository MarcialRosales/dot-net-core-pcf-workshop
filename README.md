PCF .Net Core Developers workshop
==

<!-- TOC depthFrom:1 depthTo:6 withLinks:1 updateOnSave:1 orderedList:0 -->

- [Introduction](#Introduction)
- [Pivotal Cloud Foundry Technical Overview](#pivotal-cloud-foundry-technical-overview)
- [Deploying simple apps](#deploying-simple-apps)
  - [Lab - Deploy .Net Core app](#deploy-dot-net-app)
  - [Lab - Deploy web site](#deploy-web-site)
  - [Quick Introduction to Buildpacks](#quick-intro-buildpack)
  - [Deploying applications with application manifest](#deploying-applications-with-application-manifest)
  - [Platform guarantees](#Platform-guarantees)
- [Cloud Foundry services](#cloud-foundry-services)
  - [Lab - Load flights from a database](#load-flights-from-a-provisioned-database)  


<!-- /TOC -->
# Introduction

## Prerequisites

- Java JDK 1.8
- Maven 3.3.x
- Latest git client and ideally your own github account but not essential
- `curl` or `Postman` (https://www.getpostman.com/) or similar http client. Otherwise you can use the browser, ideally with some plugin to render JSON.

### Login to Cloud Foundry's Apps Manager

You must have received an email with the credentials (username, password) to access Cloud Foundry. Go to the URL provided by the instructor. Upon a successful login, you are entering a web application called **Apps Manager**.

### Install Cloud Foundry Command-Line Interface (cli)

To install the CLI you can either go to the **Tools** option in the right hand side menu of the **Apps Manager** and follow the instructions in the page. Or you can download it from https://docs.cloudfoundry.org/cf-cli/install-go-cli.html.

### Login to Cloud Foundry thru CLI

Go to the **Tools** option in the **Apps Manager** and copy the `cf login` command and execute from a Terminal. Enter your username and password.
You are probably prompted to select one organization. Select the organization that matches your name.

At this point, you are logged into PCF and are ready to push applications to your targeted environment.

> Targeted environment: PCF is organized into several organizations and each organization into one or many spaces. We must select the organization and space where we want to deploy apps.  

# Pivotal Cloud Foundry Technical Overview

Reference documentation:
- https://docs.pivotal.io
- [Elastic Runtime concepts](http://docs.pivotal.io/pivotalcf/concepts/index.html)


# Deploying simple apps

CloudFoundry excels at the developer experience: deploy, update and scale applications on-demand regardless of the application stack (java, php, node.js, go, etc).  We are going to learn how to deploy 4 types of applications: .Net Core app and static web pages.

Reference documentation:
- [Using Apps Manager](http://docs.pivotal.io/pivotalcf/1-9/console/index.html)
- [Using cf CLI](http://docs.pivotal.io/pivotalcf/1-9/cf-cli/index.html)
- [Deploying Applications](http://docs.pivotal.io/pivotalcf/1-9/devguide/deploy-apps/deploy-app.html)
- [Deploying with manifests](http://docs.pivotal.io/pivotalcf/1-9/devguide/deploy-apps/manifest.html)

In the next sections we are going to deploy a ASP.NET Core MVC application and a web site. Before we proceed with the next sections we are going to checkout the following repository which has the .Net projects we are about to deploy.

1. `git clone https://github.com/MarcialRosales/dot-net-pcf-workshops.git`
2. `cd dot-net-pcf-workshops`

## <a name="deploy-dot-net-app"></a> Deploy a ASP.NET Core MVC app
This application was created using the out of the box ASP.NET Core MVC template found in the dotnet CLI. It uses MVC to render the home page and it also has REST controllers that allows us to look up flights by its origin and destination.

1. `cd load-flights-from-in-memory-db/flight-availability`
3. Build the app  
  `dotnet restore`  
  `dotnet build`  
  `dotnet publish -o publish -r ubuntu.14.04-x64`  
4. Deploy the app  
  `cf push flight-availability -p publish --random-route`
6. Check out application's details, whats the url?  
  `cf app flight-availability`  
7. Go to the application's home page   
8. Test the application's REST api:
  `curl 'https://<yourAppURL>/api?origin=MAD&destination=ACE'`



## <a name="deploy-web-site"></a> Deploy a web site
Deploy static site associated to the flight availability and make it internally available on a given private domain.
The static site corresponds to the API documentation of our flight-availability application. We use [ReDoc](https://github.com/Rebilly/ReDoc) to produce a web site from a Swagger definition that we have downloaded from our running application (`/swagger/v1/swagger.json`) and saved as `swagger.yml`.

1. Assuming you are under `load-flights-from-in-memory-db`
2. Deploy the app  
  `cf push flight-availability-api -p flight-availability-api-doc --random-route`  use this command if you build it
3. Check out application's details, whats the url?  
  `cf app flight-availability-site`  
4. How did PCF know that this was a static site and not a .Net application?

## <a name="quick-intro-buildpack"></a> Quick Introduction to Buildpacks

We have pushed two applications, a .Net Core and a static web site. We know that for the .Net Core we need a .Net Core Runtime and to run the static web site we need a web server like Apache or Nginx.

From [.Net buildpack](https://docs.cloudfoundry.org/buildpacks/dotnet-core/index.html#pushing-apps) ...
> Cloud Foundry automatically uses the .NET Core buildpack when one or more of the following conditions are met:

>- The pushed app contains one or more &ast;.csproj or &ast;.fsproj files.
>- The pushed app contains one or more project.json files.
>- The app is pushed from the output directory of the dotnet publish command.

> If your app requires external shared libraries that are not provided by the rootfs or the buildpack, you must place the libraries in an ld_library_path directory at the app root.

From [Static buildpack](https://docs.cloudfoundry.org/buildpacks/staticfile/#staticfile) ...
> Cloud Foundry requires a file named Staticfile in the root directory of the app to use the Staticfile buildpack with the app.

## Deploying applications with application manifest

Rather than passing a potentially long list of parameters to `cf push` we are going to move those parameters to a file so that we don't need to type them everytime we want to push an application. This file is called  *Application Manifest*.

The equivalent *manifest* file for the command `cf push flight-availability -p  publish -i 2 --hostname fa` is:

```
---
applications:
- name: flight-availability
  instances: 2
  path: publish
  host: fa
```


*Things we can do with the manifest.yml file* (more details [here](http://docs.pivotal.io/pivotalcf/1-9/devguide/deploy-apps/manifest.html))
- [ ] simplify push command with manifest files (`-f <manifest>`, `-no-manifest`)
- [ ] register applications with DNS (`domain`, `domains`, `host`, `hosts`, `no-hostname`, `random-route`, `routes`). We can register http and tcp endpoints.
- [ ] deploy applications without registering with DNS (`no-route`) (for instance, a messaging based server which does not listen on any port)
- [ ] specify compute resources : memory size, disk size and number of instances!! (Use manifest to store the 'default' number of instances ) (`instances`, `disk_quota`, `memory`)
- [ ] specify environment variables the application needs (`env`)
- [ ] as far as CloudFoundry is concerned, it is important that application start (and shutdown) quickly. If we are application is too slow we can adjust the timeouts CloudFoundry uses before it deems an application as failed and it restarts it:
	- `timeout` (60sec) Time (in seconds) allowed to elapse between starting up an app and the first healthy response from the app
	- `env: CF_STAGING_TIMEOUT` (15min) Max wait time for buildpack staging, in minutes
	- `env: CF_STARTUP_TIMEOUT` (5min) Max wait time for app instance startup, in minutes
- [ ] CloudFoundry is able to determine the health status of an application and restart if it is not healthy. We can tell it not to check or to checking the port (80) is opened or whether the http endpoint returns a `200 OK` (`health-check-http-endpoint`, `health-check-type`)
- [ ] CloudFoundry builds images from our applications. It uses a set of scripts to build images called buildpacks. There are buildpacks for different type of applications. CloudFoundry will automatically detect the type of application however we can tell CloudFoundry which buildpack we want to use. (`buildpack`)
- [ ] specify services the application needs (`services`)

## Platform guarantees

We have seen how we can scale our application (`cf scale -i #` or `cf push  ... -i #`). When we specify the number of instances, we create implicitly creating a contract with the platform. The platform will try its best to guarantee that the application has those instances. Ultimately the platform depends on the underlying infrastructure to provision new instances should some of them failed. If the infrastructure is not ready available, the platform wont be able to comply with the contract. Besides this edge case, the platform takes care of our application availability.

Let's try to simulate our application crashed. To do so go to the home page and click on the link `KillApp`.

If we have +1 instances, we have zero-downtime because the other instances are available to receive requests while PCF creates a new one. If we had just one instance, we have downtime of a few seconds until PCF provisions another instance.


# Cloud Foundry services

We are going to continue working on our **Flight Availability** application by adding more functionality. In this section we are going to look at different versions our our application which uses a `MySQL` database instead of an in-memory one, it calls external applications and integrates with other service such as **Central Registry** and **Config Server**.

.Net applications rely on the `appsettings.json` file to provide *Connection Strings* to the application such as `MySQL connection strings` or the `URL to an external application`.

In **Cloud** environments, Cloud Native applications should follow the [12 Factors](https://12factor.net/config), specially the 3rd factor that says "Environment related configuration is provided via environment variables". In Cloud Foundry, the *Connection strings* to services like databases is provided via the `VCAP_SERVICES` variable.

## Quick introduction to Services


  `cf marketplace`  Check out what services are available

  `cf marketplace -s p-mysql pre-existing-plan ...`  Check out the service details like available plans

  `cf create-service ...`   Create a service instance with the name `flight-repository`

  `cf service ...`  Check out the service instance. Is it ready to use?

  `cf env flight-availability` Check the environment variables attached to our application


## Introduction to [Steeltoe](https://steeltoe.io/) Library

Steeltoe is a library that adds a [Configuration Provider](https://github.com/SteeltoeOSS/Configuration/tree/master/src/Steeltoe.Extensions.Configuration.CloudFoundry) to our .Net Core application. This provider enables the CloudFoundry environment variables, `VCAP_APPLICATION`, `VCAP_SERVICES` and `CF_*` to be parsed and accessed as configuration data within a .NET application.

When Microsoft developed ASP.NET Core, the next generation of ASP.NET, they created a number of new `Extension` frameworks that provide services(e.g. Configuration, Logging, Dependency Injection, etc) commonly used/needed when building applications. While these `Extensions` certainly can be used in ASP.NET Core apps, they can also be leveraged in other app types including ASP.NET 4, Console Apps, UWP Apps, etc.

With Steeltoe, we have added to the Microsoft https://github.com/aspnet/Configuration[Configuration Extension providers] by adding two additional providers:

. https://github.com/SteeltoeOSS/Configuration/tree/master/src/Steeltoe.Extensions.Configuration.CloudFoundry[CloudFoundry] Configuration provider
. https://github.com/SteeltoeOSS/Configuration/tree/master/src/Steeltoe.Extensions.Configuration.ConfigServer[Config Server Client] Configuration provider

We are going to explore in the next sections the first one.


## Load flights from a provisioned database

We want to load the flights from a relational database (mysql) provisioned by the platform not an in-memory database.

1. `cd load-flights-from-db`
2. `cd flight-availability`
3. Build the app  
  `dotnet restore`
  `dotnet build`
  `dotnet publish -o publish -r ubuntu.14.04-x64`
4. Before we deploy our application to PCF we need to provision a mysql database. If we tried to push the application without creating the service we get:
	```
	...
	FAILED
	Could not find service flight-repository to bind to mr-fa
	```

  `cf marketplace`  Check out what services are available

  `cf marketplace -s p-mysql pre-existing-plan ...`  Check out the service details like available plans

  `cf create-service ...`   Create a service instance with the name `flight-repository`

  `cf service ...`  Check out the service instance. Is it ready to use?

5. Push the application using the manifest. See the manifest and observe we have declared a service:

  ```
  applications:
  - name: flight-availability
    instances: 1
    path: publish
    random-route: true
    env:
      ASPNETCORE_ENVIRONMENT: Production
    services:
    - flight-repository

  ```
  `cf push`

7. Check out the database credentials the application is using:  
  `cf env flight-availability`

8. Test the application's REST API.


## Load fares from an external application 

1. create the new web app
  ```
  mkdir fare-service
  dotnet new webapi
  dotnet restore
  ```
2. configure the listening port
  Add assembly `Microsoft.Extensions.Configuration.CommandLine` and use it like this:
  ```
    var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .Build();
    var host = new WebHostBuilder()
        .UseConfiguration(config)
    ...
  ```
  And launch it using: `dotnet run --server.urls "http://*:5001"`

3. add controller that returns an array of fares 
4. test it:
  `curl -v -X POST -H "Content-Type: application/json" http://localhost:5001/ -d '[{ "id": "123"}, { "id": "343"} ]' | jq .`

5. create `FareService` business service which calls the *fare_service* to get flights' fares.
6. add configuration section for the `fare_service` in `appsettings.Development.json` :
  ```
  ...
  "fare_service": {
    "url": "http://localhost:5001"
  }
  ...
  ```

7. run `flight-availability` with the environment `Development`:
  `ASPNETCORE_ENVIRONMENT=Development dotnet run `

To deploy to PCF:

We are going to deploy the *fare-service* as an internal application with a DNS domain which is not visible to the internet. 

1. Prepare `manifest.yml`:
  ```
  ---
  applications:
  - name: fare-service
    path: publish
    host: mr-fare-services
    domains: 
    - private-dev.chdc20-cf.solera.com
    env:
      ASPNETCORE_ENVIRONMENT: Production
  ```
2. Build 
  `dotnet publish -o publish -r ubuntu.14.04-x64`
3. Deploy
  `cf push` 

### Configure credentials of external services
If we want to deploy the flight-availability to PCF, or any environment, we need to externalize the url of the *fare-service*. We cannot keep it along with the assembly in the `appsettings.json`. 

There are 3 ways to configure services' credentials:
  - Use Environment variables:
    Pros:
    * Simple to implement  
    Cons: 
    * We cannot change them once the application starts up. 
    * If several applications share the same environment variable, should the credentials changed, we have to reset it in all applications    
  - User-Provided-Service(s):
    Pros:
    * Treat it like any other service provided by the platform.
    * If several applications share the same service, should the credentials changed, we dont have to reset it in all applications but they all have to restart though
    Cons:
    * More complex. We have to write code to inject into Steeltoe. 
  - Configuration server:
    Props:
    * Simple to implement but not simpler than environment variables
    Cons:
    * Credentials are no longer managed within the platform but elsewhere. More operations, i.e. git clone, modify setting, commit, push, refresh all apps.

### Use Environment variables
The simplest way is to override the settings defined in `appsettings.json` via *Environment variables* like this:

  `fare_service__url="http://localhost:5001" ASPNETCORE_ENVIRONMENT=Development dotnet run `

The `Configuration` library assumes that environment variables can follow this format: *SECTION_NAME*__*ATTRIBUTE*, e.g. `fare_service__url`. 

When we deploy this app to PCF, we need to declare a environment variable for the application either in the manifest or via command line:
  ```
  cf push --no-start
  cf set-env flight-availability fare_service__url http://theURL 
  cf start flight-availability
  ```


### Miscellaneous 

## Leverage SteelToe connectors to provide an info endpoint

1. Add an actuator controller mount on `mgt/`. This controller expects on its contractor a `IOptions<CloudFoundryApplicationOptions>`. 
  ```
    public ActuatorController(ILogger<ActuatorController> logger, IOptions<CloudFoundryApplicationOptions> appInfo)
    {
      ...
    }  
  ```

2. Add CloudFoundry configuration -provided by Steeltoe- as a service:
  ```
  // This method gets called by the runtime. Use this method to add services to the container.
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddCloudFoundry(Configuration);

      ...
  ```
3. Add an `/mgt/info` endpoint which returns json representation of `CloudFoundryApplicationOption`.
  ```
   // GET: mgt/info
  [HttpGet("info")]
  public CloudFoundryApplicationOptions info()
  {
      _logger?.LogDebug("requesting info");
      
      return _appInfo;
  }
  ```

## Load fares from an internal application registered with Eureka

1. Create the service registration, a.k.a. Eureka
  (http://docs.pivotal.io/spring-cloud-services/service-registry/creating-an-instance.html)
  ```
  cf marketplace -s p-service-registry

  cf create-service p-service-registry standard service-registry

  cf services

  cf service service-registry 
  ```
2. Make sure the service is ready. PCF provisions the services asynchronously. Go to the AppsManager and check the service. Check out the dashboard. Or use the command `cf service service-registry`. 

3. Prepare `manifest.yml`:
  ```
  ---
  applications:
  - name: fare-service
    path: publish
    host: mr-fare-services
    domains: 
    - private-dev.chdc20-cf.solera.com
    env:
      ASPNETCORE_ENVIRONMENT: Production
    services:
    - service-registry
  ```
4. Build 
  `dotnet publish -o publish -r ubuntu.14.04-x64`
5. Deploy
  `cf push` 
6. Check the credentials PCF has injected into our application
  `cf env fare-service`

7. Check the application has registered with Eureka. Get the url of the dashboard 
  `cf service service-registry` 

8. Update flight-availability 

TODO Add more content
