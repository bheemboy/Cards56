# Cards56 Web Application
## About this project
This is a multi-player online card game web application written in .NET core and Java Script.

## Running this application in docker (on an ubuntu server)

This docker is coded with an intention to depoy for the domain 56cards.net. That said, it can be run for other domains also. However, some of the code that refers to 56cards.net needs to be changed.

1. Install certbot
```bash
sudo apt-get remove certbot

sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot
```

2. Download `5.1.deploy-cards56.sh` and run it. Then you can access the application by going to https://65cards.net.
```bash
# Get docker-compose.yml from github
mkdir -p ~/docker
cd ~/docker
rm 5.1.deploy-cards56.sh
wget --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/5.1.deploy-cards56.sh
rm 5.2.renew-56cards.net-cert.sh
wget -P ~/docker --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/5.2.renew-56cards.net-cert.sh
chmod +x 5.*.sh

bash 5.1.deploy-cards56.sh production
```


3. Set up a cron job to autmatically renew the letsencrypt certificate by adding the following to crontab
```bash
0 6 * * thu /home/rehman/docker/5.2.renew-56cards.net-cert.sh
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
