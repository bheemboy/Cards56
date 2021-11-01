#!/bin/bash
if [ "$(whoami)" != "rehman" ]; then
        echo "Script must be run as user: rehman"
        exit 255
fi

source /home/rehman/docker/5.0.include-paths.sh

# stop and remove any running instances of cards56web
docker rm -f cards56web
# get latest version of cards56web
docker pull bheemboy/cards56web
# remove any dangling images
docker rmi $(docker images --filter "dangling=true" -q --no-trunc)

# run the container
cd ${cards56webdir}
docker-compose up -d
cd -

# Run script to generate letsencrypt certificate if needed
source "${dockerdir}/5.2.create-56cards.net-cert.sh" production

