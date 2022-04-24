# ImagePack

## Overview
ImagePack is a simple web service providing image collections for a project.
The project is organized into 3 main modules. See each section for more details.
- [**ImagePack.API**](#ImagePack.API)
- [**ImagePack.Projects**](#ImagePack.Projects)
- [**ImagePack.ProjectImages**](#ImagePack.ProjectImages)

## Technologies
ImagePack was built on Linux using Microsoft .NET 6

- [ASP.NET 6](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-6.0)
- [NUnit](https://nunit.org/)


## Getting Started
-----------------------
Clone this repo to your machine 
- using the GitHub CLI ```gh repo clone grantelgin/ImagePack```
 - or use ```https://github.com/grantelgin/ImagePack.git``` 

 Before building, confirm you have the .NET 6 SDK installed on your machine.
 - [Install on Linux](https://docs.microsoft.com/en-us/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website)
 - [Install on Mac](https://docs.microsoft.com/en-us/dotnet/core/install/macos)
 - [Install on Windows](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60)


**Build using an IDE**

If available, use JetBrains Rider, Visual Studio Code or Visual Studio to load the solution. Select the ImagePack.API as the start up project and run or debug. 


**Build using the Terminal**

To build from the command line, navigate to the folder of the ImagePack.sln and run 
 ```dotnet build ```

 Next, navigate to the ImagePack.API folder and run it using ```dotnet run```
 The default settings are to bind to localhost port 7138.

 If you need to use a different url, you can navigate to the ```ImagePack.API/bin/Debug/net6.0``` folder and run ```ImagePack.API.exe --urls "https://localhost:your-port"```

 You can confirm it is running by viewing the Swagger interface. See details in the *ImagePack.API* section.


## ImagePack.API
-----------------
This is the application layer and entry point for the ImagePack project. 
The ImagePack API includes a swagger UI for displaying API documentation in a human-friendly way.

To access the Swagger interface after building and running the project, open your browser and go to ```https://<your-localhost-url>/swagger/index.html```

The swagger interface lists all available resources with details on required arguments and provides an interface for testing the API.


**Program.cs**

This is the first class executed when the web service is started. 
This is where the web serivce is initialized and all referenced services are initialized. 

See the section under *Add service to container*. Each of the interfaces for repositories and services in ImagePack.Projects and ImagePack.Projectimages are added in order and associated with a concrete implementation. This is how dependency injection is handled in ASP.NET. Associating concrete implementations with each interface here makes it available everywhere else in the application where the interface is referenced in the clas constructor. This is also the only place that would need to be updated if the Json file repositories were to be replaced with a redis or PostGreSQL implementation. The rest of the code referencing these interfaces would continue to work without any other changes.

**appSettings.json**
See the section *imagePack*. All config entries accessed in linked services are defined here.



## ImagePack.Projects
The Projects service provides methods for accessing the list of available projects and their details such as a project name or id.

### Domain
**Models**

Only one entity is needed in the Projects Domain currently. The Project class containsin a Project name and Id. 

**Persistence**

The interface for persistence is simple for the Project domain. We only need 2 methods. One to get all projects, and one to get details for a single project. 

**Services**

Only one service is required and defines the same interface as the persistence layer.


### Persistence
One or more repository implementations can be added to the Persistence layer. The first pass uses a simple Json file repository. This should be replaced by a SQL or distributed cache layer such as redis before this would be deployed to any environment beyond testing on developer machines.

**The Json file repository.**
 See imagePackProjects.json in ImagePack.Projects.Persistence.Data. This file is set to copy to the application directory, and the API layer is configured to find the file there. The path to the repository file can be changed via configuration. See imagePack:projects:jsonFile in the appSettings.json file in ImagePack.API.

 To add new projects to the service, see [link to instructons](instructions)

To replace the Json file repository, add a new class that implements ImagePack.Projects.Domain.Repositories.IImagePackProjectsRepository,
 then in ImagePack.API.Program.cs, replace the  concrete implementation associated with the IImagePackProjectRepository, where the JsonFileREpository is currently listed.


### Services
In the ImagePack context, the Projects service is simple and implements the Domain Service, where it then just calls the repository methods by the same name. In a more complicated domain, the service could access or update multiple repositories. Consider publishing events if any collections are updated. If this needs functionality to add a Project, then the service should publish an event that other services could subscribe to. One example would be the ProjectImages service could add an empty collection of images for a new project without any additional steps in the process.


## ImagePack.ProjectImages
---------------------------
That

### Domain
**Models**

The project images domain only has one entity, the ImageLocator which provides a url for image data and the filename the data should be associated with. 


**Repositories**

Project images are organized in the context of a specific project, so the repository only needs one method, GetAllByProjectId. See ImagePack.Projects for details on getting a project Id.

**Services**

The service for the ImagePack.ProjectImages is also simple. Just one method. GetAllByProjectId. 


### Persistence
One or more repository implementations can be added to the Persistence layer. The first pass uses a simple Json file repository. This should be replaced by a SQL or distributed cache layer such as redis before this would be deployed to any environment beyond testing on developer machines.

**The Json file repository**

The Json file repository has two parts. 1. The project-image-collection-locators, and the actual image data for each project.
In this implementation, each project has a separate json file which contains links to the images associated with each project. A separate file which points to the Json file for each project is also required. **IMPORTANT**. In this implemantation, all project json files must be in the same Data folder. The Data folder is defined in the ImagePack.API appSettings.json file. See imagePack:projectImages:jsonFileRepo:imageLocatorDataDirectory

### Services
In the ImagePack context, the ProjectImages service is simple and implements the Domain Service, where it then just calls the repository methods by the same name. In a more complicated domain, the service could access or update multiple repositories. If this service is extended to support adding new images to projects, or adding new projects. 
Consider publishing events when images are added to a project. Subscribers could be notified and update their state as appropriate.





