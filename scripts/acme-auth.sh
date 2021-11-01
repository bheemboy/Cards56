#!/bin/bash
echo $CERTBOT_VALIDATION > /home/rehman/docker/ssl/acme-challenge/$CERTBOT_TOKEN
