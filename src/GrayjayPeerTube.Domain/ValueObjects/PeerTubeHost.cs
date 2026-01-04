using System.Net;
using System.Net.Sockets;

namespace GrayjayPeerTube.Domain.ValueObjects;

/// <summary>
/// Represents a normalized PeerTube host URL.
/// Handles URL normalization: lowercase, trim, strip protocol.
/// Includes SSRF protection by blocking private/internal IP addresses.
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
        if (!Uri.TryCreate(testUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid PeerTube instance URL: {testUrl}");
        }

        // SSRF Protection: Block private/internal addresses
        ValidateNotPrivateAddress(uri.Host);

        return new PeerTubeHost(host);
    }

    /// <summary>
    /// Validates that the host is not a private/internal address (SSRF protection).
    /// </summary>
    private static void ValidateNotPrivateAddress(string hostname)
    {
        // Block localhost variants
        if (hostname.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            hostname.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Access to localhost is not allowed");
        }

        // Try to parse as IP address
        if (IPAddress.TryParse(hostname, out var ipAddress))
        {
            ValidateIpAddress(ipAddress);
        }
        else
        {
            // It's a hostname - resolve and check all IPs
            // Note: This is synchronous DNS resolution; for production,
            // consider caching or async resolution
            try
            {
                var addresses = Dns.GetHostAddresses(hostname);
                foreach (var addr in addresses)
                {
                    ValidateIpAddress(addr);
                }
            }
            catch (SocketException)
            {
                // DNS resolution failed - will fail later during HTTP request
                // Don't block here to match original behavior
            }
        }
    }

    /// <summary>
    /// Validates that an IP address is not private/reserved.
    /// </summary>
    private static void ValidateIpAddress(IPAddress ipAddress)
    {
        // IPv4 loopback: 127.0.0.0/8
        if (IPAddress.IsLoopback(ipAddress))
        {
            throw new ArgumentException("Access to loopback addresses is not allowed");
        }

        // IPv6 checks
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // IPv6 loopback (::1)
            if (ipAddress.Equals(IPAddress.IPv6Loopback))
            {
                throw new ArgumentException("Access to loopback addresses is not allowed");
            }

            // IPv6 link-local (fe80::/10)
            if (ipAddress.IsIPv6LinkLocal)
            {
                throw new ArgumentException("Access to link-local addresses is not allowed");
            }

            // IPv6 site-local (deprecated but still check) (fec0::/10)
            if (ipAddress.IsIPv6SiteLocal)
            {
                throw new ArgumentException("Access to site-local addresses is not allowed");
            }

            // IPv4-mapped IPv6 addresses (::ffff:x.x.x.x) - check the mapped IPv4
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ValidateIpAddress(ipAddress.MapToIPv4());
                return;
            }
        }

        // IPv4 private ranges
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ipAddress.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10)
            {
                throw new ArgumentException("Access to private network (10.x.x.x) is not allowed");
            }

            // 172.16.0.0/12 (172.16.0.0 - 172.31.255.255)
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            {
                throw new ArgumentException("Access to private network (172.16-31.x.x) is not allowed");
            }

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
            {
                throw new ArgumentException("Access to private network (192.168.x.x) is not allowed");
            }

            // 169.254.0.0/16 (link-local)
            if (bytes[0] == 169 && bytes[1] == 254)
            {
                throw new ArgumentException("Access to link-local addresses (169.254.x.x) is not allowed");
            }

            // 0.0.0.0/8 (current network)
            if (bytes[0] == 0)
            {
                throw new ArgumentException("Access to current network (0.x.x.x) is not allowed");
            }

            // 100.64.0.0/10 (Carrier-grade NAT)
            // Can be allowed via ALLOW_CGNAT=true environment variable for testing
            if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
            {
                var allowCgnat = Environment.GetEnvironmentVariable("ALLOW_CGNAT");
                if (!string.Equals(allowCgnat, "true", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Access to carrier-grade NAT addresses is not allowed");
                }
            }

            // 192.0.0.0/24 (IETF Protocol Assignments)
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0)
            {
                throw new ArgumentException("Access to IETF protocol assignment addresses is not allowed");
            }

            // 192.0.2.0/24 (TEST-NET-1)
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2)
            {
                throw new ArgumentException("Access to documentation/test addresses is not allowed");
            }

            // 198.51.100.0/24 (TEST-NET-2)
            if (bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
            {
                throw new ArgumentException("Access to documentation/test addresses is not allowed");
            }

            // 203.0.113.0/24 (TEST-NET-3)
            if (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113)
            {
                throw new ArgumentException("Access to documentation/test addresses is not allowed");
            }

            // 224.0.0.0/4 (Multicast)
            if (bytes[0] >= 224 && bytes[0] <= 239)
            {
                throw new ArgumentException("Access to multicast addresses is not allowed");
            }

            // 240.0.0.0/4 (Reserved for future use) and 255.255.255.255 (Broadcast)
            if (bytes[0] >= 240)
            {
                throw new ArgumentException("Access to reserved addresses is not allowed");
            }
        }
    }

    public override string ToString() => NormalizedHost;

    public override bool Equals(object? obj)
    {
        return obj is PeerTubeHost other && NormalizedHost == other.NormalizedHost;
    }

    public override int GetHashCode() => NormalizedHost.GetHashCode();
}
