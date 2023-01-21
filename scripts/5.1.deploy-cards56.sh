#!/bin/bash

# Pre-install
# sudo snap install docker
# sudo groupadd docker
# usrname=$(whoami)
# sudo usermod -aG docker $usrname
#
# wget ${HOME} --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/5.1.deploy-cards56.sh
# chmod +x $HOME/5.1.deploy-cards56.sh
# mkdir -p ${HOME}/cards56web
# wget -P ${HOME}/cards56web --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/docker-compose.yml
# wget -P ${HOME}/cards56web --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/.env
# nano ${HOME}/cards56web/.env
#

if [ "$(whoami)" == "root" ]; then
  echo "Script must NOT be run as root"
  exit 1
fi

# stop and remove any running instances of cards56web
docker stop cards56web
docker rm -f cards56web
# get latest version of cards56web
docker pull bheemboy/cards56web
# remove any dangling images
docker rmi $(docker images --filter "dangling=true" -q --no-trunc)

# run the container
cd ${HOME}/cards56web
docker-compose up -d
cd -
