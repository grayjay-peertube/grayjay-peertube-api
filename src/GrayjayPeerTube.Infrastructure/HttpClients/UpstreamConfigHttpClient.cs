using System.Net.Http.Json;
using GrayjayPeerTube.Domain.Configuration;
using GrayjayPeerTube.Domain.Entities;
using GrayjayPeerTube.Domain.Exceptions;
using GrayjayPeerTube.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrayjayPeerTube.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for fetching upstream plugin configuration from plugins.grayjay.app.
/// </summary>
public class UpstreamConfigHttpClient : IUpstreamConfigProvider
{
    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpstreamConfigHttpClient> _logger;
    private readonly UpstreamConfigOptions _options;

    private const string CacheKey = "upstream-plugin-config";

    public UpstreamConfigHttpClient(
        HttpClient httpClient,
        ICacheService cacheService,
        IOptions<UpstreamConfigOptions> options,
        ILogger<UpstreamConfigHttpClient> logger)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UpstreamPluginConfig> GetUpstreamConfigAsync(
        CancellationToken cancellationToken = default)
    {
        var ttl = TimeSpan.FromSeconds(_options.CacheTtlSeconds);

        return await _cacheService.GetOrSetAsync(
            CacheKey,
            () => FetchUpstreamConfigAsync(cancellationToken),
            ttl,
            cancellationToken);
    }

    private async Task<UpstreamPluginConfig> FetchUpstreamConfigAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching upstream plugin config from: {Url}", _options.UpstreamConfigUrl);

        try
        {
            var response = await _httpClient.GetAsync(_options.UpstreamConfigUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new UpstreamConfigException(
                    $"Failed to fetch upstream plugin config: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var config = await response.Content.ReadFromJsonAsync<UpstreamPluginConfig>(
                cancellationToken: cancellationToken);

            if (config is null)
            {
                throw new UpstreamConfigException("Empty response from upstream config server");
            }

            return config;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new UpstreamConfigException(
                "Timeout fetching plugin configuration from upstream server");
        }
        catch (HttpRequestException ex)
        {
            throw new UpstreamConfigException(
                $"Upstream config fetch error: {ex.Message}", ex);
        }
        catch (UpstreamConfigException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UpstreamConfigException(
                $"Upstream config fetch error: {ex.Message}", ex);
        }
    }
}
