using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GrayjayPeerTube.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace GrayjayPeerTube.Tests.Api;

public class ValidationControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WireMockServer _wireMockServer;

    public ValidationControllerTests(WebApplicationFactory<Program> factory)
    {
        _wireMockServer = WireMockServer.Start();

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UpstreamConfig:UpstreamConfigUrl",
                $"{_wireMockServer.Url}/PeerTubeConfig.json");
        }).CreateClient();
    }

    [Fact]
    public async Task ValidatePeerTube_MissingUrl_ReturnsValidFalse()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/validatePeerTube");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ValidationResultDto>();
        result.Should().NotBeNull();
        result!.Valid.Should().BeFalse();
        result.Error.Should().Be("peerTubePlatformUrl query parameter is mandatory");
    }

    [Fact]
    public async Task ValidatePeerTube_EmptyUrl_ReturnsValidFalse()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/validatePeerTube?peerTubePlatformUrl=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ValidationResultDto>();
        result.Should().NotBeNull();
        result!.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePeerTube_AlwaysReturnsOk()
    {
        // The validation endpoint always returns 200 OK, matching Node.js behavior
        var response = await _client.GetAsync("/api/v1/validatePeerTube?peerTubePlatformUrl=nonexistent.example.com");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public void Dispose()
    {
        _wireMockServer.Stop();
        _wireMockServer.Dispose();
    }
}
