using System.Security.Cryptography;
using System.Text;

namespace GrayjayPeerTube.Domain.ValueObjects;

/// <summary>
/// Generates a plugin ID using MD5 hash of the host, formatted as a UUID.
/// Matches the original Node.js behavior exactly.
/// </summary>
public sealed class PluginId
{
    private readonly string _value;

    private PluginId(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a PluginId from a host string using MD5 hash.
    /// </summary>
    /// <param name="host">The normalized host string</param>
    /// <returns>A PluginId instance</returns>
    public static PluginId FromHost(string host)
    {
        var normalizedHost = host.ToLowerInvariant();
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(normalizedHost));

        // The Node.js code formats the MD5 hash as a UUID using regex:
        // hash.replace(/(.{8})(.{4})(.{4})(.{4})(.{12})/, '$1-$2-$3-$4-$5')
        // We must NOT use Guid constructor as it reorders bytes (little-endian).
        // Instead, format the hex string directly to match Node.js exactly.
        var hex = Convert.ToHexStringLower(hashBytes);
        var uuid = $"{hex[..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..]}";
        return new PluginId(uuid);
    }

    public override string ToString() => _value;

    public override bool Equals(object? obj)
    {
        return obj is PluginId other && _value == other._value;
    }

    public override int GetHashCode() => _value.GetHashCode();
}
