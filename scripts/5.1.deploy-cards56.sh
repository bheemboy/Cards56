#!/bin/bash
if [ "$(whoami)" != "rehman" ]; then
        echo "Script must be run as user: rehman"
        exit 255
fi

# stop and remove any running instances of cards56web
docker rm -f cards56web
# get latest version of cards56web
docker pull bheemboy/cards56web
# remove any dangling images
docker rmi $(docker images --filter "dangling=true" -q --no-trunc)

mkdir -p /home/rehman/docker/cards56web
mkdir -p /home/rehman/acme-challenge
mkdir -p /home/rehman/docker/ssl

# download files if needed
file="/home/rehman/docker/cards56web/docker-compose.yml"
if [ ! -e "$file" ]; then
    wget -P /home/rehman/docker/cards56web https://raw.githubusercontent.com/bheemboy/Cards56/master/.env
    wget -P /home/rehman/docker/cards56web https://raw.githubusercontent.com/bheemboy/Cards56/master/docker-compose.yml
    wget -P /home/rehman/docker/cards56web https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/acme-auth.sh
    wget -P /home/rehman/docker/cards56web https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/acme-cleanup.sh
    chmod +x /home/rehman/docker/cards56web/acme-auth.sh
    chmod +x /home/rehman/docker/cards56web/acme-cleanup.sh
fi

# generate self signed certificates if needed
file="/home/rehman/docker/ssl/fullchain.pem"
if [ ! -e "$file" ]; then
    openssl req -x509 -subj /CN=localhost -days 365 -set_serial 2 -newkey rsa:4096 -keyout /home/rehman/docker/ssl/privkey.pem -nodes -out /home/rehman/docker/ssl/fullchain.pem
fi

# run the container
cd /home/rehman/docker/cards56web
docker-compose up -d
cd -

# Run script to generate letsencrypt certificate if needed
source 5.2.create-56cards.net-cert.sh production

