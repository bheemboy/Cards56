# Use the following commands to build and push
# docker build -t bheemboy/cards56web2:latest -t bheemboy/cards56web2:2021.10.30 .
# docker push --all-tags bheemboy/cards56web2

# Stage 1
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet publish Cards56Web.sln -c Release -o /webapp
RUN mkdir -p /webapp/wwwroot/.well-known/acme-challenge

# Stage 2
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final

EXPOSE 80 443
ENV TZ=America/Los_Angeles
ENV ASPNETCORE_URLS=http://+:5000

WORKDIR /webapp
COPY --from=build /webapp .

COPY ./scripts /scripts
RUN chmod -R 755 /scripts/*.sh

RUN apt-get update; \
    apt-get install -y nginx certbot python3-certbot-nginx; \
    rm /etc/nginx/sites-enabled/default; \
    mkdir -p /etc/nginx/ssl; \
    openssl req -x509 -subj /CN=localhost -days 365 -set_serial 2 -newkey rsa:4096 \
        -keyout /etc/nginx/ssl/privkey.pem -nodes \
        -out /etc/nginx/ssl/fullchain.pem; \
    mkdir -p /etc/letsencrypt/renewal-hooks/deploy; \
    ln -s /scripts/01-reload-nginx.sh /etc/letsencrypt/renewal-hooks/deploy/01-reload-nginx.sh

COPY ./nginx/sites-available/56cards.net /etc/nginx/sites-available/56cards.net
RUN ln -s /etc/nginx/sites-available/56cards.net /etc/nginx/sites-enabled/56cards.net

CMD ["sh", "/scripts/startup.sh"]

HEALTHCHECK CMD curl -f http://localhost/ || exit 1
