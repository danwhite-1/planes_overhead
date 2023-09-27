# Build image with: docker build -t planes_overhead/zone_searcher -f zone_searcher/Dockerfile .
# Run with: docker run -p 5000:5000 --env-file=zone_searcher/docker.env planes_overhead/zone_searcher
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY zone_searcher/zone_searcher.csproj /app/zone_searcher/
COPY lib/FlightLib/*.csproj lib/FlightLib/*.cs /app/lib/FlightLib/
COPY lib/DBAccessLib/*.csproj lib/DBAccessLib/*.cs /app/lib/DBAccessLib/
COPY lib/SearchZoneLib/*.csproj lib/SearchZoneLib/*.cs /app/lib/SearchZoneLib/
WORKDIR /app
RUN dotnet restore zone_searcher/zone_searcher.csproj

# copy everything else and build
COPY zone_searcher/ /app/zone_searcher
WORKDIR /app/zone_searcher
RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /app
COPY --from=build-env /app/zone_searcher/out .
EXPOSE 5000
ENTRYPOINT ["dotnet", "zone_searcher.dll", "--urls", "http://0.0.0.0:5000"]