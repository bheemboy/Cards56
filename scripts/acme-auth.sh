#!/bin/bash
echo "creating.../webapp/wwwroot/.well-known/acme-challenge/"$CERTBOT_TOKEN
echo $CERTBOT_VALIDATION > /webapp/wwwroot/.well-known/acme-challenge/$CERTBOT_TOKEN

