#!/bin/bash
if [ "$(whoami)" != "rehman" ]; then
        echo "Script must be run as user: rehman"
        exit 255
fi
server=""
if [ "$1" == "production" ]; then
    server="https://acme-v02.api.letsencrypt.org/directory"
elif [ "$1" == "staging" ]; then
    server="https://acme-staging-v02.api.letsencrypt.org/directory"
else
    echo "run script with parameter 'staging' or 'production'"
    exit 255
fi

dockerdir="/home/rehman/docker"
cards56webdir="${dockerdir}/cards56web"
ssldir="${dockerdir}/ssl"
letsencryptdir="${ssldir}/letsencrypt"
livecertdir="${letsencryptdir}/config/live/56cards.net"

mkdir -p ${cards56webdir}
mkdir -p ${cards56webdir}/acme-challenge
mkdir -p ${ssldir}

# stop and remove any running instances of cards56web
docker rm -f cards56web
# get latest version of cards56web
docker pull bheemboy/cards56web
# remove any dangling images
docker rmi $(docker images --filter "dangling=true" -q --no-trunc)

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

# run the container
cd ${cards56webdir}
docker-compose up -d
cd -

# Generate letsencrypt certificate if needed
certbot certonly -n --manual --preferred-challenges=http \
      --manual-auth-hook "${cards56webdir}/acme-auth.sh" \
      --manual-cleanup-hook "${cards56webdir}/acme-cleanup.sh" \
      --email sunil.rehman@gmail.com \
      --server $server \
      --agree-tos \
      --domain "56cards.net" \
      --work-dir "${letsencryptdir}/work" \
      --config-dir "${letsencryptdir}/config" \
      --logs-dir "${letsencryptdir}/logs"

# If the certificate file is newer
if [ "${livecertdir}/fullchain.pem" -nt "${ssldir}/fullchain.pem" ]; then
    cp "${livecertdir}/fullchain.pem" "${ssldir}/fullchain.pem"
    cp "${livecertdir}/privkey.pem" "${ssldir}/privkey.pem"
    docker exec -it cards56web service nginx reload
fi
