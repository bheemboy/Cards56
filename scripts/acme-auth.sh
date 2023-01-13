#!/bin/bash
echo $CERTBOT_VALIDATION > /home/ubuntu/ssl/acme-challenge/$CERTBOT_TOKEN
