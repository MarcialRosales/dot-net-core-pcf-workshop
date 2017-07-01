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
<!--
- [Cloud Foundry services](#cloud-foundry-services)
  - [Lab - Load flights from a database](#load-flights-from-a-provisioned-database)  
  - [Lab - Load flights fares from an external application using User Provided Services](#load-flights-fares-from-an-external-application-using-user-provided-services)
  - [Lab - Let external application access a platform provided service](#let-external-application-access-a-platform-provided-service)
- [Routes and Domains](#routes-and-domains)
  - [Lab - Private and Public routes/domains](#private-and-public-routesdomains)
  - [Lab - Blue-Green deployment](#blue-green-deployment)
  - [Lab - Routing Services](#routing-services)
- [Orgs, Spaces and Users](orgsSpacesUsers-README.md)
- [Build packs](buildpack-README.md)
  - [Lab - Adding functionality](buildpack-README.md#adding-functionality)
  - [Lab - Changing functionality](buildpack-README.md#changing-functionality)
-->

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
    memory: 1024M
    path: publish
    random-route: true
    services:
    - flight-repository

  ```

7. Check out the database credentials the application is using:  
  `cf env flight-availability`

8. Test the application's REST API.
