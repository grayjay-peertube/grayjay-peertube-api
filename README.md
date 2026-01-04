# PeerTube Instances

## Description
The PeerTube Instances is a tool designed to help users discover and explore various instances of the PeerTube video hosting platform and easily add them to the Grayjay app.

It provides a user-friendly interface for browsing PeerTube instances, displaying key metrics such as the number of users, videos, and instance versions.

## Components
- **Frontend**: The frontend of the application is built using HTML, CSS, and JavaScript. It utilizes libraries such as Bootstrap, DataTables, and SweetAlert2 to create a responsive and interactive user interface.
- **Backend**: The backend is implemented in .NET 10 using ASP.NET Core. It handles API requests for fetching PeerTube instance data and plugin configuration for Grayjay integration.
- **Integration with Grayjay**: The application integrates with Grayjay, a PeerTube client, allowing users to easily add discovered instances to their Grayjay instance.

## Project Structure

```
├── public/                 # Frontend static files (HTML, CSS, JS)
├── dotnet/                 # .NET backend
│   ├── src/
│   │   ├── GrayjayPeerTube.Api/           # ASP.NET Core Web API
│   │   ├── GrayjayPeerTube.Application/   # Business logic & services
│   │   ├── GrayjayPeerTube.Domain/        # Domain models & interfaces
│   │   └── GrayjayPeerTube.Infrastructure/# HTTP clients & caching
│   ├── tests/
│   │   └── GrayjayPeerTube.Tests/         # xUnit tests
│   └── docker/
│       ├── Dockerfile
│       └── docker-compose.yml
```

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /` | Serves the frontend application |
| `GET /version` | Returns API version |
| `GET /api/v1/validatePeerTube?peerTubePlatformUrl=` | Validates a PeerTube instance |
| `GET /api/v1/PluginConfig.json?peerTubePlatformUrl=` | Returns Grayjay plugin configuration |

## Usage

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (optional, for containerized deployment)

### Development

```bash
cd dotnet

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
cd dotnet/docker

# Build and run
docker-compose up --build

# Or for production
docker-compose -f docker-compose.yml up --build
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_URLS` | Server binding URLs | `http://+:8080` |
| `PluginConfig__Protocol` | Protocol for generated URLs | `https` |
| `PluginConfig__ConfigHost` | Host for generated URLs | Request host |
| `UpstreamConfig__CacheTtlSeconds` | Cache TTL for upstream config | `10` |
