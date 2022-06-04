#!/bin/bash
function usage {
        echo "Usage: $(basename $0) [-s] [-t TAG]" 2>&1
        echo 'Deploy card56 docker image.'
        echo '   -s          Use letsencrypt staging server. Production server is used by default'
        echo '   -t TAG      Specify a specific tag to deploy. 'latest' is used by default'
        exit 1
}

if [ "$(whoami)" != "rehman" ]; then
        echo "Script must be run as user: rehman"
        exit 1
fi

server="https://acme-v02.api.letsencrypt.org/directory"
tag="latest"

# Define list of arguments expected in the input
optstring=":st:"

while getopts ${optstring} arg; do
  case ${arg} in
    s)
      server="https://acme-staging-v02.api.letsencrypt.org/directory"
      ;;
    t)
      tag="${OPTARG}"
      ;;
    ?)
      echo "Invalid option: -${OPTARG}."
      echo
      usage
      ;;
  esac
done

echo "server : $server"
echo "tag   : $tag"

dockerdir="/home/rehman/docker"
cards56webdir="${dockerdir}/cards56web"
ssldir="${dockerdir}/ssl"
letsencryptdir="${ssldir}/letsencrypt"
livecertdir="${letsencryptdir}/config/live/56cards.net"

mkdir -p ${cards56webdir}
mkdir -p ${ssldir}/acme-challenge

# stop and remove any running instances of cards56web
docker stop cards56web
docker rm -f cards56web
# get latest version of cards56web
docker pull bheemboy/cards56web:${tag}
# remove any dangling images
docker rmi $(docker images --filter "dangling=true" -q --no-trunc)

# re-download files
rm ${cards56webdir}/.env; wget -P ${cards56webdir} --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/.env
rm ${cards56webdir}/docker-compose.yml; wget -P ${cards56webdir} --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/docker-compose.yml
rm ${ssldir}/acme-auth.sh ; wget -P ${ssldir} --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/acme-auth.sh
rm ${ssldir}/acme-cleanup.sh ;wget -P ${ssldir} --no-cache https://raw.githubusercontent.com/bheemboy/Cards56/master/scripts/acme-cleanup.sh
chmod +x ${ssldir}/acme-auth.sh
chmod +x ${ssldir}/acme-cleanup.sh

# generate self signed certificates if needed
if [ ! -e ${ssldir}/fullchain.pem ]; then
    openssl req -x509 -subj /CN=localhost -days 365 -set_serial 2 -newkey rsa:4096 -keyout ${ssldir}/privkey.pem -nodes -out ${ssldir}/fullchain.pem
fi

# run the container
cd ${cards56webdir}
sed -r -i "s/CARDS56WEB_TAG=.*/CARDS56WEB_TAG=$tag/g" .env
docker-compose up -d
cd -

# Generate letsencrypt certificate if needed
certbot certonly -n --manual --preferred-challenges=http \
      --manual-auth-hook "${ssldir}/acme-auth.sh" \
      --manual-cleanup-hook "${ssldir}/acme-cleanup.sh" \
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
