using GrayjayPeerTube.Application.Services;
using GrayjayPeerTube.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrayjayPeerTube.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<PluginConfigOptions>(
            configuration.GetSection(PluginConfigOptions.SectionName));

        // Register services
        services.AddScoped<IPeerTubeValidationService, PeerTubeValidationService>();
        services.AddScoped<IPluginConfigService, PluginConfigService>();

        return services;
    }
}
