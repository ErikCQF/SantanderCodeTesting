using Microsoft.Extensions.Options;
using SantanderCodeTesting.InfraStructure;
using SantanderCodeTesting.Model;
using System.Threading;

namespace SantanderCodeTesting.Services;

public class FetcherException : Exception
{
    public string FriendlyMessage = string.Empty;
}
public sealed class FetcherHttpClient : IFetcher
{
    private readonly ILogger<FetcherHttpClient> _logger;
    private readonly IOptions<HttpClientSettings> _options;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore;

    public FetcherHttpClient(ILogger<FetcherHttpClient> logger,
                             IOptions<HttpClientSettings> options,
                             HttpClient httpClient)
    {
        _logger = logger;
        _options = options;
        _semaphore = new(_options.Value.MaxHttpCalls, _options.Value.MaxHttpCalls);
        _httpClient = httpClient;
    }


    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        // according to documentation it returns the best Stories
        var ids = await _httpClient.GetFromJsonAsync<List<int>>("beststories.json", cancellationToken);
        return ids ?? new List<int>();
    }

    public async Task<HackerItemData?> GetItemAsync(int id, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _httpClient.GetFromJsonAsync<HackerItemData>($"item/{id}.json", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Hacker News item {ItemId}", id);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
