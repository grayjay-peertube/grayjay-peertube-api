using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GrayjayPeerTube.Tests.Api;

public class HealthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetVersion_ReturnsVersion()
    {
        // Act
        var response = await _client.GetAsync("/version");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<VersionResponse>();
        result.Should().NotBeNull();
        result!.Version.Should().Be("2.0.0");
    }

    [Fact]
    public async Task GetVersion_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/version");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    private record VersionResponse(string Version);
}
