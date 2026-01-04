# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY ["GrayjayPeerTube.sln", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/GrayjayPeerTube.Api/GrayjayPeerTube.Api.csproj", "src/GrayjayPeerTube.Api/"]
COPY ["src/GrayjayPeerTube.Application/GrayjayPeerTube.Application.csproj", "src/GrayjayPeerTube.Application/"]
COPY ["src/GrayjayPeerTube.Domain/GrayjayPeerTube.Domain.csproj", "src/GrayjayPeerTube.Domain/"]
COPY ["src/GrayjayPeerTube.Infrastructure/GrayjayPeerTube.Infrastructure.csproj", "src/GrayjayPeerTube.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/GrayjayPeerTube.Api/GrayjayPeerTube.Api.csproj"

# Copy source code
COPY ["src/", "src/"]

# Build and publish
WORKDIR /src/src/GrayjayPeerTube.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Use built-in non-root user (available since .NET 8)
USER $APP_UID

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD wget -q --spider http://localhost:8080/version || exit 1

ENTRYPOINT ["dotnet", "GrayjayPeerTube.Api.dll"]
