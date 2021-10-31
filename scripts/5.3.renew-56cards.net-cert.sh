#!/bin/bash

/usr/bin/certbot renew -q --post-hook "/snap/bin/docker exec -it cards56web service nginx reload" \
                       --work-dir /home/rehman/docker/cards56web/letsencrypt/work \
                       --config-dir /home/rehman/docker/cards56web/letsencrypt/config \
                       --logs-dir /home/rehman/docker/cards56web/letsencrypt/logs

# cron-job
# 0 6 * * thu /home/rehman/docker/5.3.renew-56cards.net-cert.sh
