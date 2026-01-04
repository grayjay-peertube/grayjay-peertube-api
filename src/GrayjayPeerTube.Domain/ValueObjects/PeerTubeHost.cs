namespace GrayjayPeerTube.Domain.ValueObjects;

/// <summary>
/// Represents a normalized PeerTube host URL.
/// Handles URL normalization: lowercase, trim, strip protocol.
/// </summary>
public sealed class PeerTubeHost
{
    public string NormalizedHost { get; }
    public Uri BaseUri { get; }

    private PeerTubeHost(string normalizedHost)
    {
        NormalizedHost = normalizedHost;
        BaseUri = new Uri($"https://{normalizedHost}");
    }

    /// <summary>
    /// Creates a PeerTubeHost from a URL string, applying normalization.
    /// </summary>
    /// <param name="url">The URL to normalize (can include or exclude protocol)</param>
    /// <returns>A normalized PeerTubeHost instance</returns>
    /// <exception cref="ArgumentException">Thrown when URL is null, empty, or invalid</exception>
    public static PeerTubeHost FromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("peerTubePlatformUrl query parameter is mandatory");
        }

        // Normalize: lowercase and trim
        var host = url.Trim().ToLowerInvariant();

        // Strip protocol if present (matching Node.js behavior exactly)
        if (host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            host = host[8..];
        }
        else if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            host = host[7..];
        }

        // Validate URL format by attempting to construct a URI
        var testUrl = $"https://{host}";
        if (!Uri.TryCreate(testUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid PeerTube instance URL: {testUrl}");
        }

        return new PeerTubeHost(host);
    }

    public override string ToString() => NormalizedHost;

    public override bool Equals(object? obj)
    {
        return obj is PeerTubeHost other && NormalizedHost == other.NormalizedHost;
    }

    public override int GetHashCode() => NormalizedHost.GetHashCode();
}
