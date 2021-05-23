#!/bin/bash

docker rm -f cards56web

# Note: ServerIP should be replaced with your external ip.
docker run --cap-add=NET_ADMIN -d \
    --name cards56web \
    -p 5000:5000/tcp -p 5001:5001/tcp \
    -v /home/rehman/ssl/config/live/thuruppugulan.net:/https:ro \
    -e "ASPNETCORE_URLS=https://+:5001;http://+:5000" \
    -e "ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword" \
    -e "ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx" \
    --dns=1.1.1.1 \
    --restart=always \
    bheemboy/cards56web
