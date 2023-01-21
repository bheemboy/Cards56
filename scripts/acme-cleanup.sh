#!/bin/bash
echo "deleting.../webapp/wwwroot/.well-known/acme-challenge/"$CERTBOT_TOKEN
rm -f /webapp/wwwroot/.well-known/acme-challenge/$CERTBOT_TOKEN
