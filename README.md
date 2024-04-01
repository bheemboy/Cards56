# Cards56 Web Application
## What is Cards56
This is a multi-player online card game called 56.

## How to use the image

To start the container type:
```bash
docker run -d -p 8080:80 nextcloud
```

You can also run this docker using a docker-compose 

```bash
version: "3"
services:
    web:
        image: bheemboy/cards56web
        container_name: cards56web
        restart: unless-stopped
        ports:
            - "80:80"
```

Once started you can access the game by browsing to http://localhost/

## Setting up development environment for Windows

1. Download and install asp.net core 8 sdk
```
winget install Microsoft.DotNet.SDK.8
```

2. Download and install Visual Studio Code
```
winget install Microsoft.VisualStudioCode
```

3. Clone the source code
```
git clone https://github.com/bheemboy/Cards56.git
cd Cards56
```
4. You should be now ready to load and run/debug the application in VSCode.
