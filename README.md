PCF .Net Core Developers workshop
==

<!-- TOC depthFrom:1 depthTo:6 withLinks:1 updateOnSave:1 orderedList:0 -->

- [Introduction](#Introduction)
- [Pivotal Cloud Foundry Technical Overview](#pivotal-cloud-foundry-technical-overview)
- [.Net framework vs .Net core & PCF](#framework-comparison)
  - ASP.NET Core vs ASP.NET 4.x
  - .Net Core and ASP.NET Core
  - .Net Core on Linux Cells
  - Basic Introduction to ASP.NET Core
  - .NET 4.x support on Cloud Foundry
  - Containers: Garden-Linux vs Garden-windows
- [Deploying .Net apps](#deploying-simple-apps)
  - [Lab 1 - Deploy simple ASP.NET MVC app](#lab1)
  - [Lab 2 - Deploy simple ASP.NET Core MVC app](#lab2)
  - [Lab 3 - Understand how and on which URL our application is listening on](#lab3)
- [Quick Introduction to Buildpacks](#quick-intro-buildpack)
- [Deploying applications with application manifest](#deploying-applications-with-application-manifest)
- [Platform guarantees](#Platform-guarantees)
- Deploying other apps
  - [Lab 4 - Deploy static web site](#lab4)
- [Basic Logging and Monitoring](#logging)
  - [Lab 5 - Improve logging](#lab5)  
- [Troubleshooting hints](#troubleshooting-hints)

- [Introduction to Cloud Foundry services](#cloud-foundry-services)
  - [Lab 6 - Getting service credentials from environment with Steeltoe (.Net)](#lab7)
  - [Lab 7 - Getting service credentials from environment with Steeltoe (.Net Core)](#lab7)
  - [Lab - Load flights from a database](#load-flights-from-a-provisioned-database)  
  - [Service Brokers](#service-brokers)

- [Architectural and development best practices: 12 factor](#12factor)
- [Developing Cloud Native apps with Spring Cloud Services/Steeltoe](#scs)
  - [Service Registration and Discovery](#eureka)
    -	Lab: Discover service via Service Registration

  - [Configuration Management](#configserver)
    -	Lab: Use Centralized configuration

- [Domains & Routing Services](#domains)
  -	[Private and public routess](#domains1)
  -	[Blue/Green deployment](#domains2)
  -	[Router Service](#domains3)



<!-- /TOC -->
# Introduction

## Prerequisites

- .Net 4.5 SDK
- .NET Core SDK 1.1
- Java JDK 1.8
- Maven 3.3.x
- Latest git client and ideally your own github account but not essential
- `curl` or `Postman` (https://www.getpostman.com/) or similar http client. Otherwise you can use the browser, ideally with some plugin to render JSON.

### Login to Cloud Foundry's Apps Manager

You must have received an email with the credentials (username, password) to access Cloud Foundry. Go to the URL provided by the instructor. Upon a successful login, you are entering a web application called **Apps Manager**.

### Install Cloud Foundry Command-Line Interface (cli)

To install the CLI you can either go to the **Tools** option in the right hand side menu of the **Apps Manager** and follow the instructions in the page. Or you can download it from https://docs.cloudfoundry.org/cf-cli/install-go-cli.html.

During this workshop we will be using the `cf` command-line. It is really important that we get familiar with this tool. However, it is interesting to know that there are Extensions for Visual Studio: [Visual Studio Extension for Cloud Foundry](https://github.com/cloudfoundry/cf-vs-extension).


### Login to Cloud Foundry thru CLI

Go to the **Tools** option in the **Apps Manager** and copy the `cf login` command and execute from a Terminal. Enter your username and password.
You are probably prompted to select one organization. Select the organization that matches your name.

At this point, you are logged into PCF and are ready to push applications to your targeted environment.

> Targeted environment: PCF is organized into several organizations and each organization into one or many spaces. We must select the organization and space where we want to deploy apps.  

# Pivotal Cloud Foundry Technical Overview

Reference documentation:
- https://docs.pivotal.io
- [Elastic Runtime concepts](http://docs.pivotal.io/pivotalcf/concepts/index.html)

# Considerations for designing and running an application in the cloud

- Automated provisioning & configuration
- Automated scaling
- Infrastructure independence
- Continuous delivery
- Loose coupling
- Rapid recovery
- DevOps

# <a name="framework-comparison"></a> .NET framework vs .NET Core & PCF

## ASP.NET Core vs ASP.NET 4.x
- ASP.Net Core has web api, asp.net mvc, but no web forms or signalR.
- ASP.Net 4.6 battle-tested, hardened, it has all apis
- Pivotal opinion is to go for .Net Core based on some visibility Pivotal has on the technology.
- There is a significant refactoring to go from .net 4.x to .net core. It is not a trivial move.
- Oracle driver for .Net Core is not available until end of 2017.
- Best path forward (most people care about their existing apps = brownfield): Start with your .Net 4.x app, and bring new .net core libraries into our app. It will work fine .. Once the other libraries in our project (such as Entity Frameowkr or oracle) become available in the .net core, we can switch entirely to .net core.
- For greenfield projects: start with .net core, target CLR to use .Net core, if need to use library or feature only on .Net, switch TargetFramework to net462.

You should use .NET Core for your server application when:
* You have cross-platform needs.
* You are targeting microservices (because .Net core is lighter and its API surface can be minimized)
* You are using Docker containers (.Net core more suitable due to its smaller footprint)
* You need high performance and scalable systems. (.NET core and ASP.NEt core are the best options)
* You need side by side of .NET versions by application. (This is not so much of an issue in PCF because applications are deployed on their own, i.e. they are not deployed on the same OS/Container)

You should use .NET Framework for your server application when:
* Your application currently uses .NET Framework (recommendation is to extend instead of migrating)
* You need to use third-party .NET libraries or NuGet packages not available for .NET Core. For instance, ASP.NET WEb Forms is not available on ASP.NET Core. Or EF Core 1.1 does not have all the features available in EF 6.x.
* You need to use .NET technologies that are not available for .NET Core.
* You need to use a platform that doesn’t support .NET Core.
* Additional reasons:
  * Asynchronous programming is the default mode for ASP.NET Core and EF Core


> Useful: https://stackify.com/15-lessons-learned-while-converting-from-asp-net-to-net-core/
> Useful: Check this out on how to build windows services which are platform agnostic https://stackify.com/creating-net-core-windows-services/


## .Net Core and ASP.NET Core

- Runs across platforms Windows, Linux, OSX (including tooling called `dotnet` cli)
- Modular - built on NuGet packaging system. Enables *a la carte* .Net development
- .NET core framework libraries are an API Subset of .NET framework
- ASP.NET Core is separate from .NET core
- Features:
  - Produces self-contained apps (with `dotnet publish`), self-hosting (it is just a console app with an embedded web server)  
  - Dependency injection out of the box
  - Middleware handles request processing (routing, static files, session, authentication, etc)
  - New Configuration framework which separates application config from `web.config` and enables multiple configuration sources
  - Startup class : Builds configuration, Configure services in the DI container, and Configures Middleware.
  - Logging out of the box

## .Net Core on Linux Cells
- .NET Core not shared among applications. App1 can run on .NET Core 1.0 and App2 on .NET core 1.1 on the same cell.
- Isolation provided by Linux Containers

## Basic ASP.NET Core Introduction

### Bootstrap
Prior to ASP.NET Core, ASP.NET Framework applications were loaded by IIS. The executable InetMgr.exe creates and calls a managed web application's entry point. The HttpApplication.Application_Start() event is fired during this initialization process. A developer's first chance to execute code was to handle the Application_Start event in Global.asax.

ASP.Net Core app is a standalone/console app which comes with a built-in server called Kestrel which is estimated to be 20x times faster than IIS. It is similar to self-hosted web api projects with Owin.

Everything starts in `Program.cs` which builds a host using `WebHostBuilder`, i.e. configures web server, listening address, and specify the `Startup` class.

Then we have the `Startup` class which builds the configuration in its constructor and has 2 methods: `ConfigureServices` which creates services and registers them in the container. We use `IServiceCollection` to register services. And `Configure` which configures the middleware pipeline.

### Dependency Injection out of the box
ASP.NET Core Dependency Injection is not as advanced like Auofac but it is fundamental to the operation of ASP.NET core. To add a service provided by some assemblies we call their respective extended methods like `addMvc` or `addCloudFoundry`. To add our own service instances so that we can inject them into the controllers, we do it like this `addSingleton<IMyService, MyService>()`.

### Application Configuration Management
ASP.NET Core has redesigned how environment configuration is managed. The biggest benefit of this new configuration system is that it’s not based on System.Configuration or web.config. Instead, it pulls from an ordered set of configuration providers that support a variety of file formats (such as XML and JSON) as well as environment variables.

> What’s nice about this model is that it’s not integrated in to ASP.NET Core 1.0 as an intrinsic feature but rather implemented as NuGet packages. The same occurs with the built-in DI support as a separate nuget package (Microsoft.Extensions.DependencyInjection).

These new packages (Microsoft.Extensions.Configuration) support up to .NET 4.5.1.
It’s possible to use the configuration system independently from dependency injection. But if you combine it with IC we get injection of POCO classes into our services' constructors.

## .NET 4.x support on Cloud Foundry

- Always push to Windows Cell (`stack: windows2012R2`)
  - .NET 4.x shared by all container processes
  - Applications run within their own HWC (Hosted Web Core) and user account
  - Disk quotas assigned to each user account
- Supported application types:
  - ASP.NET - MVC, WebForm, WebAPI, WCF • ASP.NET OWIN based apps
  - ASP.NET Core web apps targeting .NET Framework
  - .NET “Background processes”: Commandline/Consoleapps


## Containers: Garden-Linux vs Garden-windows
- Garden is an API used by CF to interact with the container technology. There is one implementation for Linux and another for Windows.
- Garden-linux containers are based on LXC
- Garden-Windows is very similar to LXC but not identical so the behavior varies.
  * It has process isolation primitives similar to LXC using Windows 2012R2 Job Objects.
  * Container provided with unique user, home directory ACL’d to prevent access.
  * File system isolation achieved by creating a temporary account per container and leveraging ACL in Windows. Files in `c:\containerzer` are private to container. ANy other folder is shared across all containers.
  * Network isolation: Applications bind directly to the external IP of the cell. Internal container ports must be mapped to external ports (http://engineering.pivotal.io/post/windows-containerization-deep-dive/)
  * Memory isolation: Uses job object process isolation + the "Guard". Guard monitors app memory usage and kill jobs if `mapped memory > allocated memory`. Guard also enforces no process can be launched outside the job object process tree.

# Deploying .Net apps

CloudFoundry excels at the developer experience: deploy, update and scale applications on-demand regardless of the application stack (java, php, node.js, go, etc).  We are going to learn how to deploy 3 types of applications: .Net framework, .Net Core app and static web pages.

Reference documentation:
- [Using Apps Manager](http://docs.pivotal.io/pivotalcf/1-9/console/index.html)
- [Using cf CLI](http://docs.pivotal.io/pivotalcf/1-9/cf-cli/index.html)
- [Deploying Applications](http://docs.pivotal.io/pivotalcf/1-9/devguide/deploy-apps/deploy-app.html)
- [Deploying with manifests](http://docs.pivotal.io/pivotalcf/1-9/devguide/deploy-apps/manifest.html)

Before we proceed with the next sections we are going to checkout the following repository which has the .Net projects we are about to deploy.

1. `git clone https://github.com/MarcialRosales/dot-net-core-pcf-workshops.git`
2. `cd dot-net-core-pcf-workshops`

# Considerations for Designing and Running an Application in the Cloud

Applications written in any of the supported application frameworks often run unmodified on Cloud Foundry, if the application design follows a few simple guidelines:
  - Avoid Writing to the Local File System: shorted lived and not shared
  - Port consideration: http, https, Websockets (log aggregator) and recently we added support on tcp
  - Ignore Unnecessary Files When Pushing: `.cfignore`, `.DS_Store`, `.git`, `.gitignore`. The best practice is to exclude them using a `.cfignore` file.
  - Run Multiple Instances to Increase Availability
  - Using Buildpacks
  - Avoid the use of GAC - Global Assembly Cache - Public/Shared assembly


**Overview of the deployment process**:
1. Prepare to deploy
  - cloud ready?
  - include all required dependencies
  - exclude unnecessary files
  - does CF support our application?
2. Target your environment
  - api endpoint
  - credentials
  - org and space
3. Configure Domains (optional)
  - which is the URL the application will listen on
4. Determine deployment options  
  - number of instances
  - health check type
  - memory and disk limit
  - environment variables
  - services
5. Push the app



## <a name="lab1"></a> Lab 1 - Deploy simple ASP.NET MVC app

Let's start deploying our first application to PCF. We are going to deploy an ASP.NET MVC app.
We can either create the simple application following the steps below, or directly go to the folder `simple-apps/3-sample-dotnet`, open the project in Visual Studio and follow step 5 onwards.

1. In Visual Studio, Create New project
2. Select ASP.NET Web Application (.Net Framework)
3. Give it the name `3-sample-dotnet`
4. Select MVC template. No authentication.
5. Ensure project is built (on Windows): > Build menu > Publish option > Select publish to folder
6. From the output folder we run  
  `cf push 3-sample -s windows2012R2`.

  Once the application has successfully deployed, PCF shows us in the console the following information:
  ```
  requested state: started
  instances: 1/1
  usage: 1G x 1 instances
  urls: 3-sample.cfapps.pez.pivotal.io
  last uploaded: Wed Aug 30 09:08:11 UTC 2017
  stack: windows2012R2
  buildpack: hwc_buildpack

  state     since                         cpu    memory        disk          details
  #0   running   2017-08-30 11:08:38 AM   0.0%   43.6M of 1G   24.8M of 1G
  ```

7. Lets go to the browser and check our application. Which is the URL?


> NOTE: If hwc_buildpack is not installed, point the buildpack directly to the gitHub location. For Windows cells: must point to a zip file for now.

> NOTE: (From GSS playbook) PCF 1.10: .NET apps must use the HWC buildpack or a custom buildpack that offers the same functionality; standalone/executable apps must use the binary buildpack with a start command

> NOTE: We don't need to indicate the buildpack. PCF chooses the [hwc_buildpack](https://github.com/cloudfoundry/hwc-buildpack). It uses the HWC, which is a wrapper around [Hosted Web Core API](https://msdn.microsoft.com/en-us/library/ms693832(v=vs.90).aspx) for running .NET Applications on Windows.

### Windows cells
Windows stemcells are just like Linux based stemcells with the obvious difference being they run Windows Server instead of Linux.
Some terminology you should be familiar with before continuing:
- *Stemcell* - a versioned Operating System image wrapped with IaaS specific packaging that serves as a base template for cells.
- *Cell* - an instance of a stemcell or VM running inside Cloud Foundry that runs various container workloads.

A Windows stemcell contains a bare minimum Windows Server 2012R2 OS with a few Windows features:
- Latest Windows updates from Microsoft.
- Hostable Web Core and supporting features installed.
- Latest .NET 4.x installed.
- Latest BOSH agent.
- Recommended security policies applied.

*Cells as Immutable Infrastructure* - Windows cells are running instances of a specific stemcell version. Think of a stemcell like a static template that all instantiated cells are based off. Windows cells, as opposed to Regular Windows Server, is that Windows cells follow the immutable server pattern.


### Recommendation - .NET Versions
It’s recommended that the latest version of .NET be installed on your stemcell. Unfortunately the way the .NET 4.x framework and CLR are installed is globall at the cell level. While the mutlitude of .NET versions are generally backwards compatible, applications pushed to Cloud Foundry can only target the latest version already deployed on the cell. This is different for .NET Core where the CLR version is independently deployed along with the application and doesn’t require that it be preinstalled by a Cloud Foundry operator.


### Recommendation - Add global error handler

We should always add a global error handler (added to `Global.asax.cs` file) otherwise should the application failed to handle some requests we would not know why.

1. Add the following method to the HttpApplication found in the `Global.asax.cs` file:
  ```
  public class MvcApplication : System.Web.HttpApplication
  {
    ...
    void Application_Error(object sender, EventArgs e)
    {
        Exception lastError = Server.GetLastError();
        Console.WriteLine($"Unhandled exception: {lastError.Message} {lastError.StackTrace}");
    }
  }
  ```

### About Web.Config

Most application Web.configs work out of the box with PCF, however here are a couple of things to watch out for.

- Don’t use Windows integrated auth, it’s been disabled in PCF.
- Don’t use HTTP modules that don’t ship with .NET or can’t be deployed in your app’s bin directory, for example the [Micorsoft URL Rewrite module](https://www.iis.net/downloads/microsoft/url-rewrite).
- SQL Server connection strings must use fully qualified domain names.


## <a name="lab2"></a> Lab 2 - Deploy simple ASP.NET Core MVC app

Let's continue deploying our second application to PCF. We are going to deploy this time an ASP.NET Core MVC app. We can follow these steps to create a project from scratch or instead directly go to the folder `simple-apps/1-sample-dotnet-core` and skip step 1.

1. Create project
  ```
  mkdir 1-sample-dotnet-core
  dotnet new mvc
  ```
2. To run it locally
  ```
  dotnet restore
  dotnet run
  ```
3. Deploy it to PCF
  ```
  cf push 1-sample
  ```

> Note: This time we have not specified the stack. This is because the default stack is Linux. This means that our .Net Core application will run within a container in a Linux cell.


We should get the following output when we deploy our application:
  ```
  Creating app sample in org pivot-mrosales / space development as mrosales@pivotal.io...
  OK

  Creating route sample.cfapps.pez.pivotal.io...
  OK

  Binding sample.cfapps.pez.pivotal.io to sample...
  OK

  Uploading sample...
  Uploading app files from: /Users/mrosales/Documents/client-projects/Solera/Mexico-DotNet-workshop/cf-sample-app-dotnetcore
  Uploading 2K, 5 files
  Done uploading
  OK

  Starting app sample in org pivot-mrosales / space development as mrosales@pivotal.io...
  Downloading java_buildpack_offline_v4...
  Downloading python_buildpack...
  Downloading staticfile_buildpack
  ....
  Downloading dotnet_core_buildpack...
  Downloaded dotnet_core_buildpack
  Downloading hwc_buildpack...
  Downloaded hwc_buildpack
  Downloaded staticfile_buildpack

  Staging...
  -----> Buildpack version 1.0.22
  ASP.NET Core buildpack version: 1.0.22
  ASP.NET Core buildpack starting compile
  -----> Restoring files from buildpack cache
       OK
  -----> Restoring NuGet packages cache
       OK
  -----> Extracting libunwind
      ...
  -----> Installing .NET SDK
  -----> Restoring dependencies with Dotnet CLI

  -----> Installing required .NET Core runtime(s)
       OK
       Detected .NET Core runtime version(s) 1.0.3 required according to 'dotnet restore'
       .NET Core runtime 1.0.3 already installed
  -----> Saving to buildpack cache
       Copied 38 files from /tmp/app/libunwind to /tmp/cache
       Copied 211 files from /tmp/app/.dotnet to /tmp/cache
       Copied 8365 files from /tmp/app/.nuget to /tmp/cache
       OK
  -----> Cleaning staging area
       OK
  ASP.NET Core buildpack is done creating the droplet
  Exit status 0
  Staging complete
  Uploading droplet, build artifacts cache...
  Uploading build artifacts cache...
  Uploading droplet...
  Uploaded build artifacts cache (163.4M)
  Uploaded droplet (167.1M)
  Uploading complete
  Destroying container
  Successfully destroyed container

  0 of 1 instances running, 1 starting
  0 of 1 instances running, 1 starting
  1 of 1 instances running

  App started

  OK

  App sample was started using this command `cd . && dotnet run --project . --server.urls http://0.0.0.0:${PORT}`

  Showing health and status for app sample in org pivot-mrosales / space development as mrosales@pivotal.io...
  OK

  requested state: started
  instances: 1/1
  usage: 1G x 1 instances
  urls: sample.cfapps.pez.pivotal.io
  last uploaded: Sat Sep 2 11:28:44 UTC 2017
  stack: cflinuxfs2
  buildpack: ASP.NET Core (buildpack-1.0.22)

     state     since                    cpu     memory        disk           details
  #0   running   2017-09-02 01:30:52 PM   59.5%   39.2M of 1G   577.6M of 1G
  ```

  We are really deploying the entire source code, not the actual binary or dlls. Although this is possible and maybe from a network standpoint more efficient it is not what we want to do. We should have a pipeline that builds our artifacts and produce a binary. We should only deploy the final product, not the source code.

  Pushing source does the following:
  - Installs .NET Core runtime – version specify via global.json, else build pack chooses
  - Restores application dependencies
  - Generates the command to run the application which builds the application before it runs it

4. Test it. Which url ?

Now we are going to deploy the binaries. For that we need to configure our project with the linux runtime and use the command `dotnet publish` to produce the binaries. Follow the steps below:

1. Add the following line to the `.csproj`
  ```
  <RuntimeIdentifiers>win10-x64;osx.10.11-x64;ubuntu.14.04-x64</RuntimeIdentifiers>
  ```
2. Generate binaries for linux into the `publish` folder
  ```
  dotnet restore
  dotnet publish -o publish -r ubuntu.14.04-x64
  ```
3. Push to PCF
  ```
  cf push 1-sample -p publish
  ```

## <a name="lab3"></a> Lab 3 - Understand how and on which URL our application is listening on
We are going to explore a bit more how applications are configured to run in PCF. By default, applications listen on `http://localhost:5000` unless we configure them with the different url.

This time we are going to build a .NET Core WebAPI application. If you want to see the final result go to the folder `2-sample-dotnet-core`.

We are going to modify the app so that we log the URL it is listening on.

1. Create a new folder for our new project
  `mkdir 2-sample-dotnet-core`

2. Create project.
  `dotnet new webapi`

3. We want to push binaries so we add the runtime environment to the projects
  ```
  <PropertyGroup>
    <TargetFramework>netcoreapp1.1</TargetFramework>
    <RuntimeIdentifiers>ubuntu.14.04-x64</RuntimeIdentifiers>
  </PropertyGroup>
  ```
4. Modify the application to log the configured listening urls

5. Generate binaries for linux into the `publish` folder
  ```
  dotnet restore
  dotnet publish -o publish -r ubuntu.14.04-x64
  ```

5. Push to PCF
  ```
  cf push 2-sample -p publish
  ```

6. Check the logs and see which is the configure listening url
  `cf logs 2-sample` or `cf logs 2-sample --recent`

  We should see something like this :
  ```
  2017-09-02T17:05:47.16+0200 [APP/PROC/WEB/0] OUT Listening addresses: http://0.0.0.0:8080
  ```
7. Test the application
  `curl <url>/api/values`


# <a name="quick-intro-buildpack"></a> Quick Introduction to Buildpacks

> Note: Continue with slides "06-buildpacks.ppt"

Why buildpacks?
- Control what frameworks/runtimes are used on the platform
- Provides consistent deployments across environments:
  - Stops deployments from piling up at operation’s doorstep
  - Enables a self-service platform
- Eases ongoing operations burdens:
  - Security vulnerability is identified
  - Subsequently fixed with a new buildpack release
  - Restage applications

We have pushed two applications, a .Net Core and a static web site. We know that for the .Net Core we need a .Net Core Runtime and to run the static web site we need a web server like Apache or Nginx.

From [.Net buildpack](https://docs.cloudfoundry.org/buildpacks/dotnet-core/index.html#pushing-apps) ...
> Cloud Foundry automatically uses the .NET Core buildpack when one or more of the following conditions are met:

>- The pushed app contains one or more &ast;.csproj or &ast;.fsproj files.
>- The pushed app contains one or more project.json files.
>- The app is pushed from the output directory of the dotnet publish command.

> If your app requires external shared libraries that are not provided by the rootfs or the buildpack, you must place the libraries in an ld_library_path directory at the app root.

From [Static buildpack](https://docs.cloudfoundry.org/buildpacks/staticfile/#staticfile) ...
> Cloud Foundry requires a file named Staticfile in the root directory of the app to use the Staticfile buildpack with the app.


# Deploying applications with application manifest

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

# Platform guarantees

We have seen how we can scale our application (`cf scale -i #` or `cf push  ... -i #`). When we specify the number of instances, we create implicitly creating a contract with the platform. The platform will try its best to guarantee that the application has those instances. Ultimately the platform depends on the underlying infrastructure to provision new instances should some of them failed. If the infrastructure is not ready available, the platform wont be able to comply with the contract. Besides this edge case, the platform takes care of our application availability.

https://docs.developer.swisscom.com/devguide/deploy-apps/healthchecks.html

Let's try to simulate our application crashed. To do so go to the home page and click on the link `KillApp`.

If we have +1 instances, we have zero-downtime because the other instances are available to receive requests while PCF creates a new one. If we had just one instance, we have downtime of a few seconds until PCF provisions another instance33.

The platform uses the health-check-type configured for the application in order to monitor and determine its status.  There are several types of `health-check-type`: `process` (only necessary when we dont expose any TPC port), `port` (default) and `http` (it must expose a http port and must return 200 OK. We can specify the endpoint with `--endpoint <path>`).

# Deploying other applications

## <a name="lab4"></a> Lab 4 - Deploy a static web site
To demonstrate how we can deploy a static web site we are going to deploy the API documentation of an application we are going to be work on the next labs called *flight-availability* application.

> How we produce the API docs: We used [ReDoc](https://github.com/Rebilly/ReDoc) to produce a web site from a Swagger definition that we have downloaded from our running application (`/swagger/v1/swagger.json`) and saved as `swagger.yml`.

1. Assuming you are under `load-flights-from-in-memory-db/flight-availability-api-doc` folder
2. Deploy the app  
  `cf push flight-availability-api -p flight-availability-api-doc --random-route`  
3. Check out application's details, whats the url?  
  `cf app flight-availability-api`  
4. How did PCF know that this was a static site and not a .Net application?



# <a name="logging"></a>Basic Logging and Monitoring

> Note: Continue with slides "03-deploy-simple-app.ppt" section "Application Logs and Events"

In the lab 3 we use the standard logging API and the *Console* implementation that dumped the logs to the standard output. If we don't intend to use any centralized logging solution like ELK or Splunk, the current logging configuration is more than enough. The only issue is that .Net uses more than one line to log any statement and sometimes they intertwine.

However, if we intend to use ELK to process our logs and trigger some logic, raise alarms, etc, we need to improve our logging. Every log statement must produce a single statement not many. One way to achieve that is by using a different logging implementation like NLog. NLog allows us to control the layout of the logging statement and also to log it using a single line. log4net is another common logging framework in .Net but I have not evaluated it.

    https://docs.developer.swisscom.com/devguide/deploy-apps/streaming-logs.html


## <a name="lab5"></a>Lab 5 - Improve logging

Let's use the project `2-sample-dotnet-core` to replace `Console` logger by `Nlog`.

1. We add the dependency for NLog (`Install-Package NLog.Config`)
  ```
  <PackageReference Include="NLog" Version="5.0.0-beta07" />
  <PackageReference Include="NLog.Web.AspNetCore" Version="4.3.1" />
  ```
2. We add the following `nlog.config` file to the root of our project
  ```
  <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

    <!-- This is the layout of the logging statements -->
    <variable name="verbose" value="${longdate} | ${processid} | ${processname} | ${threadid} | ${level} | ${logger} | ${message} | ${exception:format=ToString}"/>
    <variable name="verbose_inline" value="${replace:inner=${verbose}:searchFor=\\r\\n|\\n:replaceWith=-NL-:regex=true}"/>

    <extensions>
      <add assembly="NLog.Web.AspNetCore"/>
    </extensions>

    <!-- This is the appender of the logging statements : Console using the layout we defined above -->
    <targets>
      <target name="console" xsi:type="ColoredConsole" layout="${verbose_inline}" />
    </targets>

    <!-- These are the logging levels and where we are logging to. We always log to Console -->
    <rules>
      <logger name="FlightAvailability.*" minlevel="Info" writeTo="console" />
      <logger name="*" minlevel="Warning" writeTo="console" />

    </rules>
  </nlog>
  ```
3. We configure Nlog in the `Startup.cs` class and disable the previous logging configuration.
  ```
  public Startup(IHostingEnvironment env)
  {
    ....

    env.ConfigureNLog("nlog.config");
  }
  ....

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, FlightContext ctx)
  {
      //            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      //            loggerFactory.AddDebug();

      // add NLog to ASP.NET Core instead of default Console ILogger
      loggerFactory.AddNLog();
      app.AddNLogWeb();

      ....
  }
  ```


## Interesting - HealthCheck package

The Microsoft.AspNetCore.HealthChecks package targets netcoreapp1.0, but I suspect this will change to be either netcoreapp2.0 or netstandard2.0 by the time this RTM’s.  The ASP.NET 4 project targets net461, and all the other libraries target netstandard1.3which works with both .NET Core and Full Framework. (https://scottsauber.com/2017/05/22/using-the-microsoft-aspnetcore-healthchecks-package/)

https://github.com/dotnet-architecture/HealthChecks

It is not as nice as Spring Boot Actuator project. Maybe because it is not finished yet but the fact that we have to explicitly declare all the checks in the `Startup` makes it less appealing.


## <a name="troubleshooting-hints"></a> Troubleshooting hints

### Find the cell(s) an application is running on
Sometimes the application fails we don't really understand why it is failing. There is nothing the logs. We want to access the physical VM where the application's container is running.

  ```
  cf app APPNAME --guid
  cf curl /v2/apps/c6d1259c-8057-489e-9ac2-beaa896c2bf3/stats | jq 'with_entries(.value = .value.stats.host)'
  ```
Use the bosh vms command to correlate the IPs to BOSH job indexes or VM UUIDs.

### Download application droplet
You want to know exactly which files and directory layout our application is running with. Maybe we are testing some custom buildpack and we want to test that it has inserted all the required files.


    ```
    cf app myappname --guid
    mkdir /tmp/droplet && cd /tmp/droplet
    cf curl /v2/apps/9caddd73-706c-4f82-bb63-b1435bd6240d/droplet/download > droplet.tar.gz
    tar -zxf droplet.tar.gz
    ```

### The page cannot be displayed because an internal server error has occurred.

This usually means the app crashed before it even attempted to execute your code while parsing the web.config.

Make sure you are pushing a .NET 4.x app.

If you’re ASP.NET 4.x won’t start, try pushing your application with a clean or empty Web.config and then add in bits and pieces of configuration one block at a time until you can narrow down what’s not working. Typically it’s because of a http module or handler that is not available on the Windows cell, like the Microsoft URL Rewrite module.

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


## Container/App Environment variables

PCF uses environment variables to communicate environment's configuration such as name of the app, on which port is listening, etc.
The environment variables PCF inject on each application's container are :
- `VCAP_APPLICATION` - It contains Application attributes such as version, instance index, limits, URLs, etc.
- `VCAP_SERVICES` - It contains bound services: name, label, credentials, etc.
- `CF_INSTANCE_*` like `CF_INSTANCE_ADDR`, `CF_INSTANCE_INDEX`, etc.

Example of `VCAP_APPLICATION`:
```
￼"VCAP_APPLICATION": {
  "application_id": "95bb5b8e-3d35-4753-86ee-2d9d505aec7c", "application_name": "fortuneService",
  "application_uris": [
    "fortuneservice-glottologic-neigh.apps.testcloud.com" ],
  "application_version": "40933f4c-75c5-4c61-b369-018febb0a347", "cf_api": "https://api.system.testcloud.com",
  "limits": {
    "disk": 1024, "fds": 16384, "mem": 512
  },
  "name": "fortuneService",
  "space_id": "86111584-e059-4eb0-b2e6-c89aa260453c", "space_name": "test",
  "uris": [
    "fortuneservice-glottologic-neigh.apps.testcloud.com" ],
  "users": null,
  "version": "40933f4c-75c5-4c61-b369-018febb0a347"
}
```



## Introduction to [Steeltoe](https://steeltoe.io/) Library

Steeltoe is a library that adds a [Configuration Provider](https://github.com/SteeltoeOSS/Configuration/tree/master/src/Steeltoe.Extensions.Configuration.CloudFoundry) to our .Net Core application. This provider enables the CloudFoundry environment variables, `VCAP_APPLICATION`, `VCAP_SERVICES` and `CF_*` to be parsed and accessed as configuration data within a .NET application.

When Microsoft developed ASP.NET Core, the next generation of ASP.NET, they created a number of new `Extension` frameworks that provide services(e.g. Configuration, Logging, Dependency Injection, etc) commonly used/needed when building applications. While these `Extensions` certainly can be used in ASP.NET Core apps, they can also be leveraged in other app types including ASP.NET 4, Console Apps, UWP Apps, etc.

With Steeltoe, we have added to the Microsoft https://github.com/aspnet/Configuration[Configuration Extension providers] by adding two additional providers:

. https://github.com/SteeltoeOSS/Configuration/tree/master/src/Steeltoe.Extensions.Configuration.CloudFoundry[CloudFoundry] Configuration provider
. https://github.com/SteeltoeOSS/Configuration/tree/master/src/Steeltoe.Extensions.Configuration.ConfigServer[Config Server Client] Configuration provider

We are going to explore in the next sections the first one.


## <a name="lab6"></a> Lab 6 - Getting service credentials from environment with Steeltoe (.Net)

Rather than writing code that uses Steeltoe to create a database connection or register our application with Eureka, we are going to create a playground project. Steeltoe uses .Net Core DI framework to bootstrap itself. If we are in .Net framework we have to do some workarounds.

The purpose of this playground project is to introduce Autofac as the DI framework and populate Autofac with all the configuration and services provisioned by Steeltoe.

We can follow the steps below or jump directly to the final project under `simple-apps/ExploreSteeltoeAutofac`.

1. New MVC + Web API project
2. Add TestController
3. Add NLog
  `install-package NLog.Config`
4. Add Autofac
  `install-package Autofac`
  `install-package Autofac.WebAPI2`

5. Add platform abstraction (http://michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps)
  `install-package Microsoft.Extensions.PlatformAbstractions`
6. Add Configuration package
  `install-package Microsoft.Extensions.Configuration`

  The biggest benefit of this new configuration system (introduced since .net core) is that it’s not based on System.Configuration or web.config.  Instead, it pulls from an ordered set of configuration providers that support a variety of file formats (such as XML and JSON) as well as environment variables.
  What’s nice about this model is that it’s not integrated in to ASP.NET Core 1.0 as an intrinsic feature but rather implemented as NuGet packages.
  Because the configuration support has been implemented as NuGet packages, they can be included in other projects, not just ASP.NET Core.
  http://scottdorman.github.io/2016/03/19/integrating-asp.net-core-configuration-in-mvc-4/


## <a name="lab7"></a> Lab 7 - Getting service credentials from environment with Steeltoe (.Net Core)

We are going to do something similar but this time we are going to do it .Net core.

Some hints:

1. Add an WebAPI controller mount on `mgt/`. This controller expects on its contractor a `IOptions<CloudFoundryApplicationOptions>`.
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


## <a name="lab8"></a> Lab 8 - Load flights from a provisioned database

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


### Access Mysql database managed by PCF

We have 2 options:
1) SSH tunneling + use mysql client: https://docs.pivotal.io/pivotalcf/1-11/devguide/deploy-apps/ssh-services.html
2) Push a SQL management app like this one, https://docs.pivotal.io/p-mysql/2-0/use.html#dev-tools, and access the database thru it.


## <a name="lab8"></a> Lab 8 - Load fares from an external application

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
  - **Use Environment variables**:
    Pros:
    * Simple to implement  
    Cons:
    * We cannot change them once the application starts up.
    * If several applications share the same environment variable, should the credentials changed, we have to reset it in all applications    
  - **User-Provided-Service(s)**:
    Pros:
    * Treat it like any other service provided by the platform.
    * If several applications share the same service, should the credentials changed, we dont have to reset it in all applications but they all have to restart though
    Cons:
    * More complex. We have to write code to inject into Steeltoe.
  - **Configuration server**:
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

## Load fares from external application - Add logging

We want to leverage the standard logging library which exposes a unified api, `ILogger`.

### Add Console Logger provider

the issue with this one is that it generates multi-line logging which makes it very difficult to process logs using solutions like ELK or Splunk.

### Add NLog provider

1. Add packages to `flight-availability.csproj`:
  ```
    <PackageReference Include="NLog" Version="5.0.0-beta07" />
    <PackageReference Include="NLog.Web.AspNetCore" Version="4.3.1" />
  ```
2. Configure it in the `Startup.cs`
3. Remove previous configuration of Console logger
4. Add a new configuration file `nlog.config` which configures *NLog*.
  We have configured the logger to exclude any **new line characters** from message and we have chosen a layout that we can adjust at will.
5. Move our logging level configuration from `appsettings.json` to `nlog.config`

We can include additional fields like illustrated [here](https://stackify.com/nlog-guide-dotnet-logging#post-6968-_aiscos69sga1) .


## Load fares from external application - Global Error handling

Our api so far returns a html blob when it encounters an error. We need to transform every Exception into a json response which reports the error in some way. In addition to that we want to log the exception.

For that we are going to use add a **Middleware** class and we are going to register it in our `Startup.cs` class.
1. Add `ErrorHandlingMiddlewar`
  ```
   public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger<Startup> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<Startup> logger)
        {
            this.next = next;
            this._logger = logger;
        }
  }
  ```

2. which captures exceptions, logs them,
  ```
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
  ```
3. and transform them into a json response.
  ```
  private Dictionary<string, HttpStatusCode> handlers = new Dictionary<string, HttpStatusCode>();

  private Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
      HttpStatusCode code = handlers.ContainsKey(exception.GetType().Name) ?
          handlers[exception.GetType().Name] : HttpStatusCode.InternalServerError;

      var result = JsonConvert.SerializeObject(new { error = exception.Message });
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int)code;
      return context.Response.WriteAsync(result);
  }
  ```

To test it, shutdown the *fare-service* and requests flights with fares.
We should get a logging statement like this:
`2017-08-29 16:10:12.8800 | 51266 | dotnet | 3 | Error | FlightAvailability.Startup | HttpRequestException : An error occurred while sending the request. |`
And a json response like this:
`{"error":"An error occurred while sending the request."}`


## Service Brokers

> Use slides: 05.2-service-brokers.ppt


# <a name="12factor"></a>Architectural and development best practices: 12 factor

> Use slides: 18-design.pdf
> Share book : Beyond_the_12-Factor_App_Pivotal.pdf

- Dependency management
  For .NET, this means that your application cannot rely on facilities like the Global Assembly Cache.
  Not properly isolating dependencies can cause untold problems.
  Properly managing your application’s dependencies is all about the concept of repeatable deployments
- Design, build, release and run
  A single codebase is taken through the build process to produce a compiled artifact. This artifact is then merged with configuration information that is external to the application to produce an immutable release. The immutable release is then delivered to a cloud environment.
- Treat logs as stream of Events
  A truly cloud-native application never concerns itself with routing or storage of its output stream.
  One of the many reasons your application should not be controlling the ultimate destiny of its logs is due to elastic scalability.  
- Disposability
  A cloud-native application’s processes are disposable, which means they can be started or stopped rapidly.


# <a name="scs"></a>Developing Cloud Native apps with Spring Cloud Services/Steeltoe

## <a name="eureka"></a>Service Registration

> Use slides: 25-Spring Cloud Netflix_ Service Discovery Slides.pdf

### Load fares from an internal application registered with Eureka

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


## <a name="configserver"></a>Configuration Management

> Use slides: 27-Spring Cloud Netflix_ Circuit Breakers with Hystrix Slides.pdf

### Load fares from external application using centralized configuration

In `load-fares-from-external-app` we configured the location of `fare-service` using local configuration file like `appsettings.json`. We also learnt that we can override that setting thru environment variables (`cf set-env` or in the `manifest.yml`).

Now, we are going to move the configuration to a central location, the *Config Server*.

[Introduction to Config Server](https://github.com/MarcialRosales/spring-cloud-workshop/blob/master/docs/SpringCloudConfigSlides.pdf)

TODO explain code changes and project changes

* Create Controller : FlightController
* Create Model class : Flight
* Create DbContext : FlightContext
* Enable migrations (we need to add EF package)
  `enable-migrations`  this creates a `Migrations` folder and a `Configration.cs` file to configure  migrations if needed
  `add-migration FlightTable`  this creates a class called `FlightTable` with would create the database from scratch.`Up` method creates it and `Down` deletes them in case of rollback.
  `update-database` runs the migrations classes to create or update the database.

* Create IFlightRepository & FlightRepository
* `Install-Package Unity` : DI Container (IDependencyResolver implementation)

1. Check the config server in the market place
  `cf marketplace -s p-config-server`

2. Create a service instance using `config-server.json` file found in the folder `load-fares-from-external-app-using-configserver`
  The json file contains the service's settings. Check out the [docs](https://docs.pivotal.io/spring-cloud-services/1-4/common/config-server/configuring-with-git.html) for more details.
  `cf create-service -c config-server.json p-config-server standard config-server`

3. Make sure the service is available (`cf service config-server`). It will indicate `create in progress` or `create succeeded`.

4. Update manifest to include `config-server`

5. Build
  `dotnet publish -o publish -r ubuntu.14.04-x64`

5. Deploy
  `cf push`


## Load flights from a provisioned Mysql database using .Net and Migrations

We are going to load the `Flight` entity using Entity Framework 6 and we are going to manage the schema using *Migrations*.

These are the steps to create the project. The final project is under https://github.com/MarcialRosales/dot-net-pcf-workshop/tree/master/load-flights-from-db.

1. Add Entity Framework
  `Install-Package EntityFramework`
2. Add `FlightContext`, `FlightRepository` and `IFlightRepository` classes
3. We want to manage the schema thru *Migrations* therefore we are going to enable it. From the *Package Manager Console* we run the following command. It produces a new folder `Migrations` with a `Configuration` class with a `Seed` method that we use to populate the schema with data.
  `Enable-Migrations`
4. We add the logic that automatically creates the database schema and applies schema upgrades. We add it to the class `WebApiConfig.cs` although it should be on a dedicated class. We call this method from the `Register` method.
  if there are schema changes to apply, we should see in the logs `Running migrations against database` else `There no database schema changes`.

  ```
      private static void InitializeDataStore()
      {
          System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<FlightAvailability.Models.FlightContext,
              FlightAvailability.Migrations.Configuration>());

          var configuration = new FlightAvailability.Migrations.Configuration();
          var migrator = new System.Data.Entity.Migrations.DbMigrator(configuration);
          if (migrator.GetPendingMigrations().Any())
          {
              System.Diagnostics.Debug.WriteLine("Running migrations against database");
              migrator.Update();
          }else
          {
              System.Diagnostics.Debug.WriteLine("There no database schema changes");
          }
      }
    ```
5. We instruct *Migrations* to generate the *code* that produces the schema corresponding to the current model. *Migrations* will produce a class called `InitialFlightTable` to the `Migrations` namespace.
  `add-migration InitialFlightTable`

6. We populate the table with some data thru the `Configuration.cs` class.
  ```
     protected override void Seed(FlightAvailability.Models.FlightContext context)
     {

         if (!context.Flight.Any())
         {
             InitialFlights().ForEach(f => context.Flight.Add(f));

         }

     }
     private List<Flight> InitialFlights()
     {
         var flights = new List<Flight>
         {
             new Flight{Origin="MAD",Destination="GTW",Date="18Apr2017"},
             new Flight{Origin="MAD",Destination="FRA",Date="18Apr2017"},
             new Flight{Origin="MAD",Destination="LHR",Date="18Apr2017"},
             new Flight{Origin="MAD",Destination="ACE",Date="18Apr2017"},
             new Flight{Origin="MAD",Destination="GTW",Date="19Apr2017"},
             new Flight{Origin="MAD",Destination="FRA",Date="19Apr2017"},
             new Flight{Origin="MAD",Destination="LHR",Date="19Apr2017"},
             new Flight{Origin="MAD",Destination="ACE",Date="19Apr2017"}
         };
         return flights;
     }
  ```

We could run the application as it stands now. It would create the schema but that's it because the controller is still returning dummy data.

We need to modify it so that it uses an instance of `IFlightRepository`. We are going to use *Unity* DI container to register services like `FlightRepository` and others and inject them to the `Controller` classes.

1. We add the package to use *Unity* DI container
  `Install-Package Unity.WebAPI`
2. We modify the `FlightController` so that we inject a `IFlightRepository` thru the constructor and use it to find flights.
  ```
  [Route("api")]
   public class FlightController : ApiController
   {

       private IFlightRepository _flightService;

       public FlightController(IFlightRepository flightService)
       {
           this._flightService = flightService;
       }

       [HttpGet]
       public async Task<List<Flight>> find([FromUri, Required] string origin, [FromUri, Required] string destination)
       {
           System.Diagnostics.Debug.WriteLine($"Find {origin}/{destination}");
           return await _flightService.findByOriginAndDestination(origin, destination);


       }
   }
 ```
3. Now, we need to set up the DI container in the `Global.asax.cs` class.
  ```
  protected void Application_Start()
       {
           AreaRegistration.RegisterAllAreas();
           UnityConfig.RegisterComponents();

           ...
       }
  ```
4. We need to register our classes in UnityConfig.cs class
  ```

  ```

We are now ready to test it. Run it locally from Visual Studio and run visit the following url in the browser: `http://localhost:52025/api?origin=MAD&destination=FRA`.  We should get back 2 fights.


# <a name="domains"></a>Domains & Routing Services

> Use slides: 07.1-domains-and-routes.ppt

## <a name="domains1"></a> Private and public routes

## <a name="domains2"></a>Blue/Green deployment

Lab: Apply blue-green deployment with some of the applications we have pushed so far.

## <a name="domains3"></a>Router Service

> Use slides: 07.2-route-service.ppt
