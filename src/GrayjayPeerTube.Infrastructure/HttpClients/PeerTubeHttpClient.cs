using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using GrayjayPeerTube.Domain.Entities;
using GrayjayPeerTube.Domain.Exceptions;
using GrayjayPeerTube.Domain.Interfaces;
using GrayjayPeerTube.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GrayjayPeerTube.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for interacting with PeerTube instances.
/// </summary>
public class PeerTubeHttpClient : IPeerTubeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PeerTubeHttpClient> _logger;

    public PeerTubeHttpClient(HttpClient httpClient, ILogger<PeerTubeHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PeerTubeInstanceConfig> GetInstanceConfigAsync(
        PeerTubeHost host,
        CancellationToken cancellationToken = default)
    {
        var url = $"{host.BaseUri}api/v1/config/";
        _logger.LogDebug("Fetching PeerTube config from: {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new PeerTubeValidationException(
                    $"Failed to fetch PeerTube instance configuration: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var config = await response.Content.ReadFromJsonAsync<PeerTubeInstanceConfig>(
                cancellationToken: cancellationToken);

            if (config is null)
            {
                throw new PeerTubeValidationException("Empty response from PeerTube instance");
            }

            // Validate required fields (matching Node.js behavior)
            if (config.Instance?.Name is null || config.Instance?.ShortDescription is null)
            {
                throw new PeerTubeValidationException("Invalid PeerTube instance configuration");
            }

            return config;
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException socketEx)
        {
            throw MapSocketException(socketEx);
        }
        catch (HttpRequestException ex)
        {
            throw MapHttpException(ex);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PeerTubeValidationException("Connection timeout - PeerTube instance not responding");
        }
        catch (PeerTubeValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching PeerTube config from {Url}", url);
            throw new PeerTubeValidationException($"Invalid PeerTube instance: {ex.Message}", ex);
        }
    }

    private static PeerTubeValidationException MapSocketException(SocketException ex)
    {
        return ex.SocketErrorCode switch
        {
            SocketError.HostNotFound => new PeerTubeValidationException(
                "DNS resolution failed - PeerTube instance not found"),
            SocketError.ConnectionRefused => new PeerTubeValidationException(
                "Connection refused - PeerTube instance unavailable"),
            SocketError.TimedOut => new PeerTubeValidationException(
                "Connection timeout - PeerTube instance not responding"),
            _ => new PeerTubeValidationException($"Invalid PeerTube instance: {ex.Message}", ex)
        };
    }

    private static PeerTubeValidationException MapHttpException(HttpRequestException ex)
    {
        // Check for SSL/TLS errors
        if (ex.Message.Contains("SSL", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("TLS", StringComparison.OrdinalIgnoreCase) ||
            ex.InnerException?.Message.Contains("SSL", StringComparison.OrdinalIgnoreCase) == true ||
            ex.InnerException?.Message.Contains("TLS", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new PeerTubeValidationException(
                "SSL/TLS connection error - Invalid certificate or protocol mismatch");
        }

        // Check for DNS resolution errors
        if (ex.HttpRequestError == HttpRequestError.NameResolutionError)
        {
            return new PeerTubeValidationException(
                "DNS resolution failed - PeerTube instance not found");
        }

        // Check for connection errors
        if (ex.HttpRequestError == HttpRequestError.ConnectionError)
        {
            return new PeerTubeValidationException(
                "Connection refused - PeerTube instance unavailable");
        }

        // HTTP status code errors
        if (ex.StatusCode.HasValue)
        {
            return new PeerTubeValidationException(
                $"Failed to fetch PeerTube instance configuration: {(int)ex.StatusCode} {ex.StatusCode}");
        }

        return new PeerTubeValidationException($"Invalid PeerTube instance: {ex.Message}", ex);
    }
}
