using SantanderCodeTesting.InfraStructure;
using SantanderCodeTesting.Model;

namespace SantanderCodeTesting.Services;

public class FetcherException : Exception
{
    public string FriendlyMessage = string.Empty;
}
public sealed class FetcherHttpClient : IFetcher
{
    private readonly ILogger<FetcherHttpClient> _logger;
    private readonly HttpClient _httpClient;

    public FetcherHttpClient(ILogger<FetcherHttpClient> logger, HttpClient httpClient)
    {
        _logger = logger;
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
        try
        {
            return await _httpClient.GetFromJsonAsync<HackerItemData>($"item/{id}.json", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Hacker News item {ItemId}", id);
            throw;
        }
    }
}
