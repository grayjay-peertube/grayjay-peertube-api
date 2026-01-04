using FluentAssertions;
using GrayjayPeerTube.Domain.ValueObjects;

namespace GrayjayPeerTube.Tests.Domain;

public class PeerTubeHostTests
{
    [Theory]
    [InlineData("example.com", "example.com")]
    [InlineData("https://example.com", "example.com")]
    [InlineData("http://example.com", "example.com")]
    [InlineData("HTTPS://EXAMPLE.COM", "example.com")]
    [InlineData("  example.com  ", "example.com")]
    [InlineData("https://peertube.example.org", "peertube.example.org")]
    public void FromUrl_ValidUrl_ReturnsNormalizedHost(string input, string expected)
    {
        var host = PeerTubeHost.FromUrl(input);

        host.NormalizedHost.Should().Be(expected);
    }

    [Theory]
    [InlineData("example.com", "https://example.com/")]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("http://example.com", "https://example.com/")]
    public void FromUrl_ValidUrl_CreatesCorrectBaseUri(string input, string expectedBaseUri)
    {
        var host = PeerTubeHost.FromUrl(input);

        host.BaseUri.ToString().Should().Be(expectedBaseUri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromUrl_EmptyOrNull_ThrowsArgumentException(string? input)
    {
        var act = () => PeerTubeHost.FromUrl(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("peerTubePlatformUrl query parameter is mandatory");
    }

    [Fact]
    public void FromUrl_AlwaysUsesHttps_EvenWhenHttpProvided()
    {
        var host = PeerTubeHost.FromUrl("http://example.com");

        host.BaseUri.Scheme.Should().Be("https");
    }

    [Fact]
    public void ToString_ReturnsNormalizedHost()
    {
        var host = PeerTubeHost.FromUrl("https://EXAMPLE.COM");

        host.ToString().Should().Be("example.com");
    }

    [Fact]
    public void Equals_SameHost_ReturnsTrue()
    {
        var host1 = PeerTubeHost.FromUrl("https://example.com");
        var host2 = PeerTubeHost.FromUrl("EXAMPLE.COM");

        host1.Should().Be(host2);
    }

    [Fact]
    public void Equals_DifferentHost_ReturnsFalse()
    {
        var host1 = PeerTubeHost.FromUrl("example.com");
        var host2 = PeerTubeHost.FromUrl("other.com");

        host1.Should().NotBe(host2);
    }
}
