using System.Text.Json.Serialization;

namespace GrayjayPeerTube.Domain.Entities;

/// <summary>
/// Represents the configuration response from a PeerTube instance's /api/v1/config/ endpoint.
/// </summary>
public class PeerTubeInstanceConfig
{
    [JsonPropertyName("instance")]
    public PeerTubeInstanceInfo? Instance { get; set; }
}

public class PeerTubeInstanceInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; set; }
}
