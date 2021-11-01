#!/bin/bash
if [ "$(whoami)" != "rehman" ]; then
    echo "run script as rehman"
    exit 255
fi

source 5.0.include-paths.sh

server=""

if [ "$1" == "production" ]; then
    server="https://acme-v02.api.letsencrypt.org/directory"
elif [ "$1" == "staging" ]; then
    server="https://acme-staging-v02.api.letsencrypt.org/directory"
else
    echo "run script with parameter 'staging' or 'production'"
    exit 255
fi

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
if [ "${56cardscertdir}/fullchain.pem" -nt "${ssldir}/fullchain.pem" ]; then
    cp "${56cardscertdir}/fullchain.pem" "${ssldir}/fullchain.pem"
    cp "${56cardscertdir}/privkey.pem" "${ssldir}/privkey.pem"
    docker exec -it cards56web service nginx reload
fi
