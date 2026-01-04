using System.Text.Json.Serialization;

namespace GrayjayPeerTube.Application.DTOs;

/// <summary>
/// DTO for PeerTube instance validation result.
/// </summary>
public class ValidationResultDto
{
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    public static ValidationResultDto Success() => new() { Valid = true };

    public static ValidationResultDto Failure(string error) => new() { Valid = false, Error = error };
}
