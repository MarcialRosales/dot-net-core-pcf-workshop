PCF .Net Core Developers workshop
==

<!-- TOC depthFrom:1 depthTo:6 withLinks:1 updateOnSave:1 orderedList:0 -->

- [Introduction](#Introduction)
- [Pivotal Cloud Foundry Technical Overview](#pivotal-cloud-foundry-technical-overview)
- [Deploying simple apps](#deploying-simple-apps)
  - [Lab - Deploy Spring boot app](#deploy-spring-boot-app)
  - [Lab - Deploy web site](#Deploy-web-site)
	- [Deploying applications with application manifest](#deploying-applications-with-application-manifest)
	- [Platform guarantees](#Platform guarantees)
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

## Deploy a ASP.NET Core MVC app
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

## Deploy a web site
Deploy static site associated to the flight availability and make it internally available on a given private domain

2. Assuming you are under `apps/flight-availability`
3. Build the site. Maven literally downloads hundreds of jars to generate the maven site with all the project reports such as javadoc, sure-fire reports, and others. For this reason, there is a `site` folder which has an already site. If you have a good internet connection, try this command instead:
  `mvn site`
4. Deploy the app  
  `cf push flight-availability-site -p target/site --random-route`  use this command if you build it
	`cf push flight-availability-site -p site --random-route`  use this command if you are pushing the already built site
5. Check out application's details, whats the url?  
  `cf app flight-availability-site`  
