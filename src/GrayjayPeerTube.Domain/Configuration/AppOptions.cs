namespace GrayjayPeerTube.Domain.Configuration;

/// <summary>
/// Options for plugin config service.
/// </summary>
public class PluginConfigOptions
{
    public const string SectionName = "PluginConfig";

    /// <summary>
    /// Override protocol for generated URLs (from PROTOCOL env var).
    /// </summary>
    public string? Protocol { get; set; }

    /// <summary>
    /// Override hostname for generated URLs (from CONFIG_HOST env var).
    /// </summary>
    public string? ConfigHost { get; set; }
}

/// <summary>
/// Options for upstream config.
/// </summary>
public class UpstreamConfigOptions
{
    public const string SectionName = "UpstreamConfig";

    public string UpstreamConfigUrl { get; set; } =
        "https://plugins.grayjay.app/pre-release/PeerTube/PeerTubeConfig.json";

    public string PluginBaseUrl { get; set; } =
        "https://plugins.grayjay.app/pre-release/PeerTube";

    public string IconUrl { get; set; } =
        "https://plugins.grayjay.app/PeerTube/peertube.png";

    public int CacheTtlSeconds { get; set; } = 10;
}
