using System.Text.Json;
using System.Web;
using GrayjayPeerTube.Application.DTOs;
using GrayjayPeerTube.Domain.Configuration;
using GrayjayPeerTube.Domain.Interfaces;
using GrayjayPeerTube.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrayjayPeerTube.Application.Services;

/// <summary>
/// Service for generating plugin configurations.
/// </summary>
public class PluginConfigService : IPluginConfigService
{
    private readonly IPeerTubeClient _peerTubeClient;
    private readonly IUpstreamConfigProvider _upstreamConfigProvider;
    private readonly ILogger<PluginConfigService> _logger;
    private readonly PluginConfigOptions _options;
    private readonly UpstreamConfigOptions _upstreamOptions;

    public PluginConfigService(
        IPeerTubeClient peerTubeClient,
        IUpstreamConfigProvider upstreamConfigProvider,
        IOptions<PluginConfigOptions> options,
        IOptions<UpstreamConfigOptions> upstreamOptions,
        ILogger<PluginConfigService> logger)
    {
        _peerTubeClient = peerTubeClient;
        _upstreamConfigProvider = upstreamConfigProvider;
        _options = options.Value;
        _upstreamOptions = upstreamOptions.Value;
        _logger = logger;
    }

    public async Task<PluginConfigDto> GetPluginConfigAsync(
        string? peerTubePlatformUrl,
        string requestProtocol,
        string requestHostname,
        CancellationToken cancellationToken = default)
    {
        // Parse and normalize the URL
        var host = PeerTubeHost.FromUrl(peerTubePlatformUrl);

        _logger.LogDebug("Generating plugin config for: {Host}", host.NormalizedHost);

        // Validate and fetch instance config
        var instanceConfig = await _peerTubeClient.GetInstanceConfigAsync(host, cancellationToken);

        // Fetch upstream plugin config
        var upstreamConfig = await _upstreamConfigProvider.GetUpstreamConfigAsync(cancellationToken);

        // Generate plugin ID from host
        var pluginId = PluginId.FromHost(host.NormalizedHost);

        // Build host URL (with environment variable override)
        var hostUrl = BuildHostUrl(requestProtocol, requestHostname);

        // Build source URL
        var sourceUrl = BuildSourceUrl(host.NormalizedHost, hostUrl);

        // Build script URL
        var scriptUrl = BuildScriptUrl(upstreamConfig.ScriptUrl);

        // Build platform URL
        var platformUrl = host.BaseUri.ToString().TrimEnd('/');

        // Create the plugin config by merging upstream config with instance-specific data
        var pluginConfig = new PluginConfigDto
        {
            // Instance-specific fields (override upstream)
            Name = instanceConfig.Instance?.Name,
            Description = instanceConfig.Instance?.ShortDescription,
            Id = pluginId.ToString(),
            PlatformUrl = platformUrl,
            SourceUrl = sourceUrl,
            ScriptUrl = scriptUrl,
            IconUrl = _upstreamOptions.IconUrl,

            // Fields from upstream config
            Version = upstreamConfig.Version,
            Author = upstreamConfig.Author,
            AuthorUrl = upstreamConfig.AuthorUrl,
            AllowEval = upstreamConfig.AllowEval,
            AllowUrls = upstreamConfig.AllowUrls,
            Packages = upstreamConfig.Packages,

            // Extension data (additional fields from upstream)
            ExtensionData = upstreamConfig.ExtensionData,

            // Constants with baseUrl override
            Constants = BuildConstants(upstreamConfig.Constants, platformUrl),

            // Authentication with instance-specific URLs
            Authentication = BuildAuthentication(upstreamConfig.Authentication, platformUrl),
            AuthenticationDesktop = BuildAuthentication(upstreamConfig.AuthenticationDesktop, platformUrl)
        };

        return pluginConfig;
    }

    private string BuildHostUrl(string requestProtocol, string requestHostname)
    {
        // Environment variable overrides (matching Node.js behavior)
        var protocol = !string.IsNullOrEmpty(_options.Protocol) ? _options.Protocol : requestProtocol;
        var hostname = !string.IsNullOrEmpty(_options.ConfigHost) ? _options.ConfigHost : requestHostname;

        return $"{protocol}://{hostname}";
    }

    private static string BuildSourceUrl(string normalizedHost, string hostUrl)
    {
        // URL encode the host parameter (matching encodeURIComponent in Node.js)
        var encodedHost = HttpUtility.UrlEncode(normalizedHost);
        return $"{hostUrl}/api/v1/PluginConfig.json?peerTubePlatformUrl={encodedHost}";
    }

    private string BuildScriptUrl(string? relativeScriptUrl)
    {
        if (string.IsNullOrEmpty(relativeScriptUrl))
        {
            return $"{_upstreamOptions.PluginBaseUrl}/PeerTubeScript.js";
        }

        // Resolve relative URL against base URL
        var baseUri = new Uri(_upstreamOptions.PluginBaseUrl + "/");
        var scriptUri = new Uri(baseUri, relativeScriptUrl);
        return scriptUri.ToString();
    }

    private static PluginConstants? BuildConstants(JsonElement? upstreamConstants, string platformUrl)
    {
        var constants = new PluginConstants
        {
            BaseUrl = platformUrl
        };

        if (upstreamConstants.HasValue && upstreamConstants.Value.ValueKind == JsonValueKind.Object)
        {
            constants.ExtensionData = new Dictionary<string, JsonElement>();
            foreach (var property in upstreamConstants.Value.EnumerateObject())
            {
                if (property.Name != "baseUrl")
                {
                    constants.ExtensionData[property.Name] = property.Value.Clone();
                }
            }
        }

        return constants;
    }

    private static PluginAuthenticationDto? BuildAuthentication(
        Domain.Entities.PluginAuthentication? upstreamAuth,
        string platformUrl)
    {
        if (upstreamAuth is null)
        {
            return null;
        }

        return new PluginAuthenticationDto
        {
            // Override with instance-specific URLs
            LoginUrl = $"{platformUrl}/login",
            CompletionUrl = $"{platformUrl}/api/v1/users/me?*",

            // Keep upstream values
            HeadersToFind = upstreamAuth.HeadersToFind,
            CookiesToFind = upstreamAuth.CookiesToFind,
            ExtensionData = upstreamAuth.ExtensionData
        };
    }
}
