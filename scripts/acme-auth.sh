#!/bin/bash
echo $CERTBOT_VALIDATION > $HOME/ssl/acme-challenge/$CERTBOT_TOKEN
