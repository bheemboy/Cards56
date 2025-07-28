# Builds are made on github.com using workflows

# Alternatively, the following commands can be used to build locally and push them to dockerhub
# docker build -t bheemboy/cards56web:latest -t bheemboy/cards56web:$(Get-Date -Format "yyyy.MM.dd") .
# docker push --all-tags bheemboy/cards56web

# Stage 1 ##############################################################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /build
COPY . .
RUN dotnet publish Cards56Web.sln -c Release -o /webapp

# Stage 2 ##############################################################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

EXPOSE 80
ENV TZ=America/Los_Angeles
ENV ASPNETCORE_URLS=http://+:80

# RUN apt-get update; apt-get install -y nginx curl certbot cron
RUN apt-get update; apt-get install -y curl

WORKDIR /webapp
COPY --from=build /webapp .

CMD ["dotnet", "/webapp/Cards56Web.dll"]

HEALTHCHECK CMD curl -f http://localhost/ || exit 1
