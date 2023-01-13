# Use the following commands to build and push
# docker build -t bheemboy/cards56web:latest -t bheemboy/cards56web:2022.04.22 .
# docker push --all-tags bheemboy/cards56web

# Stage 1 ##############################################################################
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /build
COPY . .
RUN dotnet publish Cards56Web.sln -c Release -o /webapp

# create path for acme challenge
RUN mkdir -p /webapp/wwwroot/.well-known

# Copy startup scripts
COPY ./scripts/startup.sh /webapp/scripts/startup.sh
RUN chmod -R 755 /webapp/scripts/*.sh

# Copy nginx config file
COPY ./nginx/sites-available/56cards.net /webapp/56cards.net

# Stage 2 ##############################################################################
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final

EXPOSE 80 443
ENV TZ=America/Los_Angeles
ENV ASPNETCORE_URLS=http://+:5000

RUN apt-get update; apt-get install -y nginx curl

WORKDIR /webapp
COPY --from=build /webapp .

RUN ln -s /webapp/56cards.net /etc/nginx/sites-enabled/56cards.net; rm /etc/nginx/sites-enabled/default

CMD ["sh", "/webapp/scripts/startup.sh"]

HEALTHCHECK CMD curl -f http://localhost/ || exit 1
