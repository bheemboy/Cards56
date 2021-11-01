#!/bin/bash

dockerdir="/home/rehman/docker"
ssldir="${dockerdir}/ssl"
letsencryptdir="${ssldir}/letsencrypt"
livecertdir="${letsencryptdir}/config/live/56cards.net"

/usr/bin/certbot renew --work-dir ${letsencryptdir}/work \
                       --config-dir ${letsencryptdir}/config \
                       --logs-dir ${letsencryptdir}/logs

# If the certificate file is newer
if [ "${livecertdir}fullchain.pem" -nt "${ssldir}fullchain.pem" ]; then
    cp "${livecertdir}fullchain.pem" "${ssldir}fullchain.pem"
    cp "${livecertdir}privkey.pem" "${ssldir}privkey.pem"
    docker exec -it cards56web service nginx reload
fi

# cron-job
# 0 6 * * thu /home/rehman/docker/5.2.renew-56cards.net-cert.sh
