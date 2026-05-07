# Stage 1: Base runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copied directly from the current folder, no messy nested paths!
COPY ["UrlShortenerService.csproj", "./"]
RUN dotnet restore "./UrlShortenerService.csproj"

COPY . .
RUN dotnet build "./UrlShortenerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish the project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UrlShortenerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortenerService.dll"]