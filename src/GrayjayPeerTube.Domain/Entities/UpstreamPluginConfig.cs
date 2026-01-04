using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrayjayPeerTube.Domain.Entities;

/// <summary>
/// Represents the upstream plugin configuration from plugins.grayjay.app.
/// Uses JsonExtensionData to capture all fields since the structure may vary.
/// </summary>
public class UpstreamPluginConfig
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("platformUrl")]
    public string? PlatformUrl { get; set; }

    [JsonPropertyName("sourceUrl")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("scriptUrl")]
    public string? ScriptUrl { get; set; }

    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("version")]
    public int? Version { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("authorUrl")]
    public string? AuthorUrl { get; set; }

    [JsonPropertyName("constants")]
    public JsonElement? Constants { get; set; }

    [JsonPropertyName("authentication")]
    public PluginAuthentication? Authentication { get; set; }

    [JsonPropertyName("allowEval")]
    public bool? AllowEval { get; set; }

    [JsonPropertyName("allowUrls")]
    public string[]? AllowUrls { get; set; }

    [JsonPropertyName("packages")]
    public string[]? Packages { get; set; }

    /// <summary>
    /// Captures any additional properties not explicitly defined.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public class PluginAuthentication
{
    [JsonPropertyName("loginUrl")]
    public string? LoginUrl { get; set; }

    [JsonPropertyName("completionUrl")]
    public string? CompletionUrl { get; set; }

    [JsonPropertyName("headersToFind")]
    public string[]? HeadersToFind { get; set; }

    [JsonPropertyName("cookiesToFind")]
    public string[]? CookiesToFind { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
