#!/bin/bash
echo $CERTBOT_VALIDATION > /opt/ssl/acme-challenge/$CERTBOT_TOKEN
