using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrayjayPeerTube.Application.DTOs;

/// <summary>
/// DTO for the generated plugin configuration.
/// This is the response returned by the /api/v1/PluginConfig.json endpoint.
/// </summary>
public class PluginConfigDto
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
    public PluginConstants? Constants { get; set; }

    [JsonPropertyName("authentication")]
    public PluginAuthenticationDto? Authentication { get; set; }

    [JsonPropertyName("allowEval")]
    public bool? AllowEval { get; set; }

    [JsonPropertyName("allowUrls")]
    public string[]? AllowUrls { get; set; }

    [JsonPropertyName("packages")]
    public string[]? Packages { get; set; }

    /// <summary>
    /// Captures any additional properties from the upstream config.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public class PluginConstants
{
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public class PluginAuthenticationDto
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
