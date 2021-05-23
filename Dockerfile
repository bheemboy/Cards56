# Stage 1
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
RUN dotnet publish -c Release -o /webapp

# Stage 2
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final
WORKDIR /webapp
COPY --from=build /webapp .
ENTRYPOINT ["dotnet", "Cards56Web.dll"]
