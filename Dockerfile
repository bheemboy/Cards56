# Use the following commands to build and push
# docker build -t bheemboy/cards56web:latest -t bheemboy/cards56web:$(Get-Date -Format "yyyy.MM.dd") .
# docker push --all-tags bheemboy/cards56web

# Stage 1 ##############################################################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build
COPY . .
RUN dotnet publish Cards56Web.sln -c Release -o /webapp

# create path for acme challenge
# RUN mkdir -p /webapp/wwwroot/.well-known

# Copy startup scripts
# COPY ./scripts /webapp/scripts
# RUN chmod -R 755 /webapp/scripts/*.sh

# Copy nginx config file
# COPY ./nginx/sites-available/56cards.net /webapp/56cards.net

# Stage 2 ##############################################################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

EXPOSE 80
ENV TZ=America/Los_Angeles
ENV ASPNETCORE_URLS=http://+:80
# ENV EMAIL=administrator@56cards.net
# ENV CERTBOT_SERVER=https://acme-v02.api.letsencrypt.org/directory
# ENV DYN_DNS_URL=https://user:password@domains.google.com/nic/update?hostname=56cards.net

# RUN apt-get update; apt-get install -y nginx curl certbot cron
RUN apt-get update; apt-get install -y curl

WORKDIR /webapp
COPY --from=build /webapp .

# RUN ln -s /webapp/56cards.net /etc/nginx/sites-enabled/56cards.net; rm /etc/nginx/sites-enabled/default

# RUN crontab -l | { cat; echo "0 0 * * * curl \$DYN_DNS_URL\n0 6 * * thu /webapp/scripts/renew-cert.sh\n"; } | crontab -

# CMD ["sh", "/webapp/scripts/startup.sh"]

CMD ["dotnet", "/webapp/Cards56Web.dll"]

HEALTHCHECK CMD curl -f http://localhost/ || exit 1
