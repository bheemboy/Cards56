#!/bin/bash
echo "Email="$EMAIL
echo "Certbot server="$CERTBOT_SERVER
echo " Dynamic DNS URL="$DYN_DNS_URL

curl $DYN_DNS_URL

ssldir=/etc/nginx/ssl
mkdir -p ${ssldir}

# generate self signed certificates if needed
if [ ! -e ${ssldir}/fullchain.pem ]; then
    openssl req -x509 -subj /CN=localhost -days 365 -set_serial 2 -newkey rsa:4096 -keyout ${ssldir}/privkey.pem -nodes -out ${ssldir}/fullchain.pem
fi

service nginx start

# Generate letsencrpt certificate
mkdir -p /webapp/wwwroot/.well-known/acme-challenge
letsencryptdir="${ssldir}/letsencrypt"
livecertdir="${letsencryptdir}/config/live/56cards.net"
certbot certonly -n --manual --preferred-challenges=http \
      --manual-auth-hook "/webapp/scripts/acme-auth.sh" \
      --manual-cleanup-hook "/webapp/scripts/acme-cleanup.sh" \
      --email $EMAIL \
      --server $CERTBOT_SERVER \
      --agree-tos \
      --domain "56cards.net" \
      --work-dir "${letsencryptdir}/work" \
      --config-dir "${letsencryptdir}/config" \
      --logs-dir "${letsencryptdir}/logs"

# If the certificate file is newer
if [ "${livecertdir}/fullchain.pem" -nt "${ssldir}/fullchain.pem" ]; then
    cp "${livecertdir}/fullchain.pem" "${ssldir}/fullchain.pem"
    cp "${livecertdir}/privkey.pem" "${ssldir}/privkey.pem"
    service nginx reload
fi

service cron start

dotnet /webapp/Cards56Web.dll
