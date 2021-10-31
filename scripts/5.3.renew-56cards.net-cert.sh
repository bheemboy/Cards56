#!/bin/bash

/usr/bin/certbot renew --work-dir /home/rehman/docker/ssl/letsencrypt/work \
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

# cron-job
# 0 6 * * thu /home/rehman/docker/5.3.renew-56cards.net-cert.sh
