#!/bin/bash
# Script to check NGINX Config and if runable reload Nginx
# called by certbot hook after new certificate was deployed or renewed
# place into:
# /etc/letsencrypt/renewal-hooks/deploy/01-nginx.sh
# more info:
# https://tcpip.wtf/en/letsencrypt-auto-nginx-reload-on-renew-hook.htm
set -e
# TESTING Config
TMP=$(mktemp /tmp/check.XXXXXXXXXX) || { echo "Failed to create temp file"; exit 1; }
/usr/sbin/nginx -t 1>>$TMP 2>>$TMP
if grep -q "test is successful" $TMP
then
        # Config OK
        echo Config OK, reloading...
        if $(pidof systemd >/dev/null)
        then
                systemctl reload nginx
        else
                /etc/init.d/nginx reload
        fi
else
        echo Config ERROR!
fi

rm $TMP>/dev/null 2>/dev/null
