# Cards56 Web Application
## About this project
This is a multi-player online card game web application written in .NET core and Java Script.

## Running this application in docker

You can run this docker using the following docker-compose.yml 

```bash
version: "3"
services:
    web:
        image: bheemboy/cards56web
        container_name: cards56web
        restart: unless-stopped
        dns:
            - 1.1.1.1
        ports:
            - "80:80"
```

## WINDOWS: Setting up development environment for this project

1. Download and install asp.net core 8 sdk

2. Download and install Visual Studio Code

3. Clone the source code
```
git clone https://github.com/bheemboy/Cards56.git
cd Cards56
```
4. You should be now ready to load and run/debug the application in VSCode.

## Mint 20.1 (UBUNTU 20.04): Setting up development environment for this project

1. Add Microsoft package signing key to your list of trusted keys and add the package repository.
```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
```
2. Install asp.net core 8 sdk
```
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```
3. Download and install Visual Studio Code

4. Clone the source code
```
git clone https://github.com/bheemboy/Cards56.git
cd Cards56
```
5. You should be now ready to load and run/debug the application in VSCode.
