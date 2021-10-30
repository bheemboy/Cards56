#!/bin/bash

server="https://acme-staging-v02.api.letsencrypt.org/directory"

if [ "$1" == "production" ]; then
    server="https://acme-v02.api.letsencrypt.org/directory"
fi

certbot certonly -n --nginx \
      --email sunil.rehman@gmail.com \
      --agree-tos \
      --domain "56cards.net" \
      --server $server

FILE="/etc/letsencrypt/live/56cards.net/fullchain.pem"
if [[ -f $FILE ]];then
    service nginx stop
    rm /etc/nginx/ssl/fullchain.pem
    rm /etc/nginx/ssl/privkey.pem
    ln -s /etc/letsencrypt/live/56cards.net/fullchain.pem /etc/nginx/ssl/fullchain.pem
    ln -s /etc/letsencrypt/live/56cards.net/privkey.pem /etc/nginx/ssl/privkey.pem
    service nginx start
fi
