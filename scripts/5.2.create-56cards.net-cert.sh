#!/bin/bash
if [ "$(whoami)" != "rehman" ]; then
    echo "run script as rehman"
    exit 255
fi

server="https://acme-staging-v02.api.letsencrypt.org/directory"

if [ "$1" == "production" ]; then
    server="https://acme-v02.api.letsencrypt.org/directory"
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

docker exec -it cards56web service nginx stop

cp /home/rehman/docker/ssl/letsencrypt/config/live/56cards.net/fullchain.pem /home/rehman/docker/ssl/fullchain.pem
cp /home/rehman/docker/ssl/letsencrypt/config/live/56cards.net/privkey.pem /home/rehman/docker/ssl/privkey.pem

docker exec -it cards56web service nginx start

