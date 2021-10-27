# Use the following commands to build and push
# docker build -t bheemboy/cards56web:latest -t bheemboy/cards56web:2021.09.22 .
# docker push --all-tags bheemboy/cards56web

# Stage 1
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet publish Cards56Web.sln -c Release -o /webapp

# Stage 2
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final
WORKDIR /webapp
COPY --from=build /webapp .

RUN apt-get update
RUN apt-get install -y nginx
RUN rm /etc/nginx/nginx.conf
COPY ./nginx/nginx.conf /etc/nginx/nginx.conf
COPY ./nginx/conf.d/default.conf /etc/nginx/conf.d/default.conf

COPY ./startup.sh /webapp/startup.sh
RUN chmod 755 /webapp/startup.sh

CMD ["sh", "/webapp/startup.sh"]

# ENTRYPOINT ["dotnet", "Cards56Web.dll"]
HEALTHCHECK CMD curl -f http://localhost/ || exit 1
