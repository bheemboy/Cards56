# Cards56 Web Application
## About this project
This is a multi-player online card game web application written in .NET core and Java Script.

## Running this application in docker

This docker is coded with an intention to depoy for the domain 56cards.net. That said, it can be run for other domains also. However, some of the code that refers to 56cards.net needs to be changed.

1. Remove any previous instances of the container
```bash
# stop and remove any running instances of cards56web
docker rm -f cards56web

# get latest version of cards56web
docker pull bheemboy/cards56web

# remove any dangling images
docker rmi $(docker images --filter "dangling=true" -q --no-trunc)

```

3. Download `5.1.deploy-cards56.sh` and run it
```bash
# Get docker-compose.yml from github
mkdir -p ~/docker
wget -P ~/docker https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/5.1.deploy-cards56.sh
wget -P ~/docker https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/5.2.renew-56cards.net-cert.sh

chmod +x ~/docker/5.*.sh

bash ~/docker/5.1.deploy-cards56.sh

# launch docker container
docker-compose -f ~/docker/cards56web/docker-compose.yml up -d
```
You can now access the application using your browser. It will be running https using self signed certificates.

4. To get proper certificate from letsencrypt, run the following command. This assumes that you own the 56cards.net domain.
```
docker exec -it cards56web /scripts/create-cert.sh production
```

5. Set up a cron job to autmatically renew the letsencrypt certificate. I added the following to crontab
```bash
0 6 * * thu /snap/bin/docker exec -it cards56web certbot renew
``` 

## WINDOWS: Setting up development environment for this project

1. Download and install asp.net core 3.1 sdk

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
2. Install asp.net core 3.1 sdk
```
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-3.1
```
3. Download and install Visual Studio Code

4. Clone the source code
```
git clone https://github.com/bheemboy/Cards56.git
cd Cards56
```
5. You should be now ready to load and run/debug the application in VSCode.
