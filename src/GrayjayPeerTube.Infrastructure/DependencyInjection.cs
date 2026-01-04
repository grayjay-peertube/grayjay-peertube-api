using GrayjayPeerTube.Domain.Configuration;
using GrayjayPeerTube.Domain.Interfaces;
using GrayjayPeerTube.Infrastructure.Caching;
using GrayjayPeerTube.Infrastructure.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrayjayPeerTube.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<UpstreamConfigOptions>(
            configuration.GetSection(UpstreamConfigOptions.SectionName));

        // Memory cache
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // HTTP clients with typed client pattern
        // 30-second timeout matching Node.js behavior
        services.AddHttpClient<IPeerTubeClient, PeerTubeHttpClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "GrayjayPeerTube/2.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            ConnectTimeout = TimeSpan.FromSeconds(30),
            EnableMultipleHttp2Connections = true
        });

        services.AddHttpClient<IUpstreamConfigProvider, UpstreamConfigHttpClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "GrayjayPeerTube/2.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

        return services;
    }
}
