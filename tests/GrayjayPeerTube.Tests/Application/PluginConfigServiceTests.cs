using FluentAssertions;
using GrayjayPeerTube.Application.Services;
using GrayjayPeerTube.Domain.Configuration;
using GrayjayPeerTube.Domain.Entities;
using GrayjayPeerTube.Domain.Interfaces;
using GrayjayPeerTube.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GrayjayPeerTube.Tests.Application;

public class PluginConfigServiceTests
{
    private readonly Mock<IPeerTubeClient> _mockClient;
    private readonly Mock<IUpstreamConfigProvider> _mockUpstreamProvider;
    private readonly Mock<ILogger<PluginConfigService>> _mockLogger;
    private readonly PluginConfigOptions _pluginOptions;
    private readonly UpstreamConfigOptions _upstreamOptions;
    private readonly PluginConfigService _service;

    private const string PlatformUrl = "https://peertube.example.com";
    private const string RequestProtocol = "https";
    private const string RequestHostname = "config.example.com";

    public PluginConfigServiceTests()
    {
        _mockClient = new Mock<IPeerTubeClient>();
        _mockUpstreamProvider = new Mock<IUpstreamConfigProvider>();
        _mockLogger = new Mock<ILogger<PluginConfigService>>();
        _pluginOptions = new PluginConfigOptions();
        _upstreamOptions = new UpstreamConfigOptions();

        _service = new PluginConfigService(
            _mockClient.Object,
            _mockUpstreamProvider.Object,
            Options.Create(_pluginOptions),
            Options.Create(_upstreamOptions),
            _mockLogger.Object);

        _mockClient
            .Setup(c => c.GetInstanceConfigAsync(It.IsAny<PeerTubeHost>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PeerTubeInstanceConfig
            {
                Instance = new PeerTubeInstanceInfo
                {
                    Name = "Test Instance",
                    ShortDescription = "A test instance"
                }
            });
    }

    [Fact]
    public async Task GetPluginConfigAsync_WithAuthentication_OverridesUrlsWithInstanceUrls()
    {
        // Arrange
        var upstreamConfig = new UpstreamPluginConfig
        {
            Authentication = new PluginAuthentication
            {
                LoginUrl = "https://upstream.example.com/login",
                CompletionUrl = "https://upstream.example.com/api/v1/users/me?*",
                HeadersToFind = ["Authorization"],
                CookiesToFind = ["session_id"]
            }
        };

        _mockUpstreamProvider
            .Setup(p => p.GetUpstreamConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(upstreamConfig);

        // Act
        var result = await _service.GetPluginConfigAsync(PlatformUrl, RequestProtocol, RequestHostname);

        // Assert
        result.Authentication.Should().NotBeNull();
        result.Authentication!.LoginUrl.Should().Be("https://peertube.example.com/login");
        result.Authentication.CompletionUrl.Should().Be("https://peertube.example.com/api/v1/users/me?*");
        result.Authentication.HeadersToFind.Should().BeEquivalentTo(["Authorization"]);
        result.Authentication.CookiesToFind.Should().BeEquivalentTo(["session_id"]);
    }

    [Fact]
    public async Task GetPluginConfigAsync_WithAuthenticationDesktop_OverridesUrlsWithInstanceUrls()
    {
        // Arrange
        var upstreamConfig = new UpstreamPluginConfig
        {
            AuthenticationDesktop = new PluginAuthentication
            {
                LoginUrl = "https://upstream.example.com/login",
                CompletionUrl = "https://upstream.example.com/api/v1/users/me?*",
                HeadersToFind = ["Authorization"],
                CookiesToFind = ["session_id"]
            }
        };

        _mockUpstreamProvider
            .Setup(p => p.GetUpstreamConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(upstreamConfig);

        // Act
        var result = await _service.GetPluginConfigAsync(PlatformUrl, RequestProtocol, RequestHostname);

        // Assert
        result.AuthenticationDesktop.Should().NotBeNull();
        result.AuthenticationDesktop!.LoginUrl.Should().Be("https://peertube.example.com/login");
        result.AuthenticationDesktop.CompletionUrl.Should().Be("https://peertube.example.com/api/v1/users/me?*");
        result.AuthenticationDesktop.HeadersToFind.Should().BeEquivalentTo(["Authorization"]);
        result.AuthenticationDesktop.CookiesToFind.Should().BeEquivalentTo(["session_id"]);
    }

    [Fact]
    public async Task GetPluginConfigAsync_WithNullAuthentication_ReturnsNull()
    {
        // Arrange
        var upstreamConfig = new UpstreamPluginConfig
        {
            Authentication = null,
            AuthenticationDesktop = null
        };

        _mockUpstreamProvider
            .Setup(p => p.GetUpstreamConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(upstreamConfig);

        // Act
        var result = await _service.GetPluginConfigAsync(PlatformUrl, RequestProtocol, RequestHostname);

        // Assert
        result.Authentication.Should().BeNull();
        result.AuthenticationDesktop.Should().BeNull();
    }

    [Fact]
    public async Task GetPluginConfigAsync_WithBothAuthTypes_OverridesBothIndependently()
    {
        // Arrange
        var upstreamConfig = new UpstreamPluginConfig
        {
            Authentication = new PluginAuthentication
            {
                LoginUrl = "https://upstream.example.com/login",
                CompletionUrl = "https://upstream.example.com/api/v1/users/me?*",
                HeadersToFind = ["Authorization"],
                CookiesToFind = ["mobile_cookie"]
            },
            AuthenticationDesktop = new PluginAuthentication
            {
                LoginUrl = "https://upstream.example.com/login",
                CompletionUrl = "https://upstream.example.com/api/v1/users/me?*",
                HeadersToFind = ["Authorization"],
                CookiesToFind = ["desktop_cookie"]
            }
        };

        _mockUpstreamProvider
            .Setup(p => p.GetUpstreamConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(upstreamConfig);

        // Act
        var result = await _service.GetPluginConfigAsync(PlatformUrl, RequestProtocol, RequestHostname);

        // Assert
        result.Authentication.Should().NotBeNull();
        result.Authentication!.LoginUrl.Should().Be("https://peertube.example.com/login");
        result.Authentication.CookiesToFind.Should().BeEquivalentTo(["mobile_cookie"]);

        result.AuthenticationDesktop.Should().NotBeNull();
        result.AuthenticationDesktop!.LoginUrl.Should().Be("https://peertube.example.com/login");
        result.AuthenticationDesktop.CookiesToFind.Should().BeEquivalentTo(["desktop_cookie"]);
    }
}
