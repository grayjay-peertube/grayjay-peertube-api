using FluentAssertions;
using GrayjayPeerTube.Domain.ValueObjects;

namespace GrayjayPeerTube.Tests.Domain;

public class PluginIdTests
{
    [Fact]
    public void FromHost_GeneratesConsistentId()
    {
        var id1 = PluginId.FromHost("example.com");
        var id2 = PluginId.FromHost("example.com");

        id1.Should().Be(id2);
    }

    [Fact]
    public void FromHost_DifferentHosts_GenerateDifferentIds()
    {
        var id1 = PluginId.FromHost("example.com");
        var id2 = PluginId.FromHost("other.com");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void FromHost_CaseInsensitive()
    {
        var id1 = PluginId.FromHost("EXAMPLE.COM");
        var id2 = PluginId.FromHost("example.com");

        id1.Should().Be(id2);
    }

    [Fact]
    public void FromHost_ReturnsValidUuid()
    {
        var id = PluginId.FromHost("example.com");

        // Should be parseable as a GUID
        Guid.TryParse(id.ToString(), out var guid).Should().BeTrue();
        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var id = PluginId.FromHost("example.com");

        var stringValue = id.ToString();

        Guid.TryParse(stringValue, out _).Should().BeTrue();
    }
}
