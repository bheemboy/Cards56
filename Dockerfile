# !!! WARNING !!!
# You must build from parent directory using
#  docker build -t bheemboy/cards56web -f Cards56Web/Dockerfile .

# Stage 1
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
# RUN git clone https://github.com/bheemboy/Cards56Lib.git
# RUN git clone https://github.com/bheemboy/Cards56Web.git
COPY ./Cards56Lib Cards56Lib
COPY ./Cards56Web Cards56Web

RUN dotnet restore Cards56Lib/Cards56Lib.csproj
RUN dotnet restore Cards56Web/Cards56Web.csproj
RUN dotnet publish -c Release -o /webapp Cards56Web/Cards56Web.csproj

# Stage 2
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final
WORKDIR /webapp
COPY --from=build /webapp .
ENTRYPOINT ["dotnet", "Cards56Web.dll"]
