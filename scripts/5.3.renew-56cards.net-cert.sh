#!/bin/bash

source /home/rehman/docker/5.0.include-paths.sh

/usr/bin/certbot renew --work-dir ${letsencryptdir}/work \
                       --config-dir ${letsencryptdir}/config \
                       --logs-dir ${letsencryptdir}/logs

# If the certificate file is newer
if [ "${56cardscertdir}fullchain.pem" -nt "${ssldir}fullchain.pem" ]; then
    cp "${56cardscertdir}fullchain.pem" "${ssldir}fullchain.pem"
    cp "${56cardscertdir}privkey.pem" "${ssldir}privkey.pem"
    docker exec -it cards56web service nginx reload
fi

# cron-job
# 0 6 * * thu /home/rehman/docker/5.3.renew-56cards.net-cert.sh
