#!/bin/bash

echo "Renew SSL Certificate..."
ssldir=/etc/nginx/ssl
letsencryptdir="${ssldir}/letsencrypt"
livecertdir="${letsencryptdir}/config/live/56cards.net"

/usr/bin/certbot renew --work-dir ${letsencryptdir}/work \
                       --config-dir ${letsencryptdir}/config \
                       --logs-dir ${letsencryptdir}/logs

# If the certificate file is newer
if [ "${livecertdir}/fullchain.pem" -nt "${ssldir}/fullchain.pem" ]; then
    cp "${livecertdir}/fullchain.pem" "${ssldir}/fullchain.pem"
    cp "${livecertdir}/privkey.pem" "${ssldir}/privkey.pem"
    service nginx reload
fi
