# Stage 1: Base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# We include the folder paths here so the GitHub root context can find it!
COPY ["UrlShortenerService/UrlShortenerService.csproj", "UrlShortenerService/"]
RUN dotnet restore "./UrlShortenerService/UrlShortenerService.csproj"

COPY . .
WORKDIR "/src/UrlShortenerService"
RUN dotnet build "./UrlShortenerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UrlShortenerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortenerService.dll"]