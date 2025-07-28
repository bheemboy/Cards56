# Cards56 Web Application
This is a multi-player online card game called 56.

## How to use the image

Run this docker using a docker-compose 

```bash
services:
    web:
        image: bheemboy/cards56web
        container_name: cards56web
        restart: unless-stopped
        ports:
            - "80:80"
```

Once started you can access the game by browsing to http://localhost/

## Setting up development environment

This guide will help you set up and build this .NET 8 project Visual Studio Code dev containers.

### Prerequisites
Before you begin, ensure you have the following installed on your machine:

- **Visual Studio Code** - [Download here](https://code.visualstudio.com/)
- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop/)
- **Dev Containers extension** for VS Code - Install from the VS Code Extensions marketplace

### Getting Started

#### Step 1: Clone and Open the Project

1. Open Visual Studio Code
2. Open the Command Palette (`Ctrl+Shift+P` on Windows/Linux, `Cmd+Shift+P` on Mac)
3. Type and select: `Dev Containers: Clone Repository in Container Volume`
4. Enter the GitHub repository URL:
   ```
   https://github.com/yourusername/your-repository-name
   ```
5. Press Enter and wait for the container to build

#### Step 2: Container Setup

The dev container will automatically:
- Set up .NET 8 LTS environment
- Install gettext tools for localization (.po to .mo file compilation)
- Restore NuGet packages (Newtonsoft.Json, NGettext)
- Configure Docker access for building production images

This process may take a few minutes on first setup.

## Running the project while developing

In the Terminal 

```bash

cd Cards56Web
dotnet run

```

You should be able to open the project at http://localhost:8080

## Building for production

### Building Docker Images - 1
This is done from github.com using github workflows. It needs to be triggered maunally. 

### Building Docker Images - 2
You may also build it manually and publish it to dockerhub

```bash
# Build with latest tag and date-based tag
docker build -t bheemboy/cards56web:latest -t bheemboy/cards56web:$(date +"%Y.%m.%d") .

# push to dockrehub
docker push --all-tags bheemboy/cards56web
```

### Building the Project for test purposes
You may want to build it for test purposes.

```bash
# Build the project
dotnet build

# Build for release
dotnet build --configuration Release

# Publish the application
dotnet publish -c Release -o ./publish
```

