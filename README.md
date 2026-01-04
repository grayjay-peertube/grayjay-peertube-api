# PeerTube Instances API

## Description
A .NET 10 ASP.NET Core Web API that validates PeerTube instances and generates Grayjay plugin configurations.

## Project Structure

```
├── src/
│   ├── GrayjayPeerTube.Api/           # ASP.NET Core Web API
│   ├── GrayjayPeerTube.Application/   # Business logic & services
│   ├── GrayjayPeerTube.Domain/        # Domain models & interfaces
│   └── GrayjayPeerTube.Infrastructure/# HTTP clients & caching
├── tests/
│   └── GrayjayPeerTube.Tests/         # xUnit tests
└── docker/
    └── docker-compose.yml
```

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /version` | Returns API version |
| `GET /health` | Health check endpoint |
| `GET /api/v1/validatePeerTube?peerTubePlatformUrl=` | Validates a PeerTube instance |
| `GET /api/v1/PluginConfig.json?peerTubePlatformUrl=` | Returns Grayjay plugin configuration |

## Usage

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (optional, for containerized deployment)

### Development

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run --project src/GrayjayPeerTube.Api

# Run tests
dotnet test
```

The application will be available at `http://localhost:5000`.

### Docker

```bash
cd docker

# Build and run
docker-compose up --build
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_URLS` | Server binding URLs | `http://+:8080` |
| `PluginConfig__Protocol` | Protocol for generated URLs | `https` |
| `PluginConfig__ConfigHost` | Host for generated URLs | Request host |
| `UpstreamConfig__CacheTtlSeconds` | Cache TTL for upstream config | `10` |
