#!/bin/bash
if [ "$(whoami)" != "rehman" ]; then
        echo "Script must be run as user: rehman"
        exit 255
fi

dockerdir="/home/rehman/docker"
cards56webdir="${dockerdir}/cards56web"
ssldir="${dockerdir}/ssl"
letsencryptdir="${ssldir}/letsencrypt"
56cardscertdir="${letsencryptdir}/config/live/56cards.net/"

mkdir -p ${cards56webdir}
mkdir -p ${cards56webdir}/acme-challenge
mkdir -p ${ssldir}

# download files if needed
file=${cards56webdir}/docker-compose.yml
if [ ! -e $file ]; then
    wget -P ${cards56webdir} https://raw.githubusercontent.com/bheemboy/Cards56/master/.env
    wget -P ${cards56webdir} https://raw.githubusercontent.com/bheemboy/Cards56/master/docker-compose.yml
    wget -P ${cards56webdir} https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/acme-auth.sh
    wget -P ${cards56webdir} https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/acme-cleanup.sh
    chmod +x ${cards56webdir}/acme-auth.sh
    chmod +x ${cards56webdir}/acme-cleanup.sh
fi

# generate self signed certificates if needed
file=${ssldir}/fullchain.pem
if [ ! -e $file ]; then
    openssl req -x509 -subj /CN=localhost -days 365 -set_serial 2 -newkey rsa:4096 -keyout ${ssldir}/privkey.pem -nodes -out ${ssldir}/fullchain.pem
fi

