#!/bin/bash

docker rm -f cards56web

# Note: ServerIP should be replaced with your external ip.
docker run --cap-add=NET_ADMIN -d \
    --name cards56web \
    -p 5000:80/tcp -p 5001:443/tcp \
    -v /home/rehman/ssl/config/live/56cards.net:/https:ro \
    -e "ASPNETCORE_URLS=https://+;http://+" \
    -e "ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword" \
    -e "ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx" \
    --dns=1.1.1.1 \
    --restart=always \
    bheemboy/cards56web
