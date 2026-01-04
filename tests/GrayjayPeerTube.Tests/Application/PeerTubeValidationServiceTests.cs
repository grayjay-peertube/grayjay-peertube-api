using FluentAssertions;
using GrayjayPeerTube.Application.Services;
using GrayjayPeerTube.Domain.Entities;
using GrayjayPeerTube.Domain.Exceptions;
using GrayjayPeerTube.Domain.Interfaces;
using GrayjayPeerTube.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GrayjayPeerTube.Tests.Application;

public class PeerTubeValidationServiceTests
{
    private readonly Mock<IPeerTubeClient> _mockClient;
    private readonly Mock<ILogger<PeerTubeValidationService>> _mockLogger;
    private readonly PeerTubeValidationService _service;

    public PeerTubeValidationServiceTests()
    {
        _mockClient = new Mock<IPeerTubeClient>();
        _mockLogger = new Mock<ILogger<PeerTubeValidationService>>();
        _service = new PeerTubeValidationService(_mockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateInstanceAsync_ValidInstance_ReturnsSuccess()
    {
        // Arrange
        var config = new PeerTubeInstanceConfig
        {
            Instance = new PeerTubeInstanceInfo
            {
                Name = "Test Instance",
                ShortDescription = "A test instance"
            }
        };

        _mockClient
            .Setup(c => c.GetInstanceConfigAsync(It.IsAny<PeerTubeHost>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        var result = await _service.ValidateInstanceAsync("example.com");

        // Assert
        result.Valid.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ValidateInstanceAsync_MissingUrl_ReturnsFailure()
    {
        // Act
        var result = await _service.ValidateInstanceAsync(null);

        // Assert
        result.Valid.Should().BeFalse();
        result.Error.Should().Be("peerTubePlatformUrl query parameter is mandatory");
    }

    [Fact]
    public async Task ValidateInstanceAsync_EmptyUrl_ReturnsFailure()
    {
        // Act
        var result = await _service.ValidateInstanceAsync("");

        // Assert
        result.Valid.Should().BeFalse();
        result.Error.Should().Be("peerTubePlatformUrl query parameter is mandatory");
    }

    [Fact]
    public async Task ValidateInstanceAsync_ClientThrows_ReturnsFailure()
    {
        // Arrange
        _mockClient
            .Setup(c => c.GetInstanceConfigAsync(It.IsAny<PeerTubeHost>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PeerTubeValidationException("Connection timeout - PeerTube instance not responding"));

        // Act
        var result = await _service.ValidateInstanceAsync("example.com");

        // Assert
        result.Valid.Should().BeFalse();
        result.Error.Should().Be("Connection timeout - PeerTube instance not responding");
    }
}
