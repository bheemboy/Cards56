#!/bin/bash
if [ "$(whoami)" != "rehman" ]; then
    echo "run script as rehman"
    exit 255
fi

server="https://acme-staging-v02.api.letsencrypt.org/directory"

if [ "$1" == "production" ]; then
    server="https://acme-v02.api.letsencrypt.org/directory"
elif [ "$1" == "staging" ]; then
    server="https://acme-v02.api.letsencrypt.org/directory"
else
    echo "run script with parameter 'staging' or 'production'"
    exit 255
fi

certbot certonly -n --manual --preferred-challenges=http \
      --manual-auth-hook /home/rehman/docker/cards56web/acme-auth.sh \
      --manual-cleanup-hook /home/rehman/docker/cards56web/acme-cleanup.sh \
      --email sunil.rehman@gmail.com \
      --server $server \
      --agree-tos \
      --domain "56cards.net" \
      --work-dir /home/rehman/docker/ssl/letsencrypt/work \
      --config-dir /home/rehman/docker/ssl/letsencrypt/config \
      --logs-dir /home/rehman/docker/ssl/letsencrypt/logs

# If the certificate file is newer
newcertpath="/home/rehman/docker/ssl/letsencrypt/config/live/56cards.net/"
existingcertpath="/home/rehman/docker/ssl/"
if [ "${newcertpath}fullchain.pem" -nt "${existingcertpath}fullchain.pem" ]; then
    cp "${newcertpath}fullchain.pem" "${existingcertpath}fullchain.pem"
    cp "${newcertpath}privkey.pem" "${existingcertpath}privkey.pem"
    docker exec -it cards56web service nginx reload
fi
