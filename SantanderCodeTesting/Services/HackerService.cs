using Microsoft.Extensions.Options;
using SantanderCodeTesting.InfraStructure;
using SantanderCodeTesting.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SantanderCodeTesting.Services;

public sealed class HackerService : IHackerService
{
    private readonly ILogger<IHackerService> _logger;
    private readonly IOptions<HackerServiceSettings> _options;
    private readonly IFetcher _fetcher;

    public HackerService(ILogger<HackerService> logger,
                        IOptions<HackerServiceSettings> options,
                        IFetcher fetcher)
    {
        _logger = logger;
        _options = options;
        _fetcher = fetcher;
    }

    public async Task<HackerItemDataResponse> GetBestStories(int bestN, CancellationToken cancellationToken)
    {
        var hackerItemIds = (await _fetcher.GetBestStoryIdsAsync(cancellationToken))
                           .Take(bestN)
                           .ToList();

        Math.Min(_options.Value.MaxConcurrentCalls, bestN);

        int batchSize = Math.Min(_options.Value.MaxConcurrentCalls, bestN);


        if (batchSize <= 0)
        {
            batchSize = 1;
        }
        try
        {
            var results = new HackerItemData?[hackerItemIds.Count];

            int offset = 0;

            foreach (var batch in hackerItemIds.Chunk(batchSize))
            {
                var tasks = batch.Select(id => _fetcher.GetItemAsync(id, cancellationToken));
                var batchResults = await Task.WhenAll(tasks);

                for (int i = 0; i < batchResults.Length; i++)
                {
                    results[offset + i] = batchResults[i];
                }
                offset += batchResults.Length;
            }
            // TODO: not sure if the hacker api can return null could return nulls, protective code
            Array.Sort(results,
                  (a, b) =>
                  {
                      if (a is null && b is null) return 0;
                      if (a is null) return 1;
                      if (b is null) return -1;

                      return b.Score.CompareTo(a.Score);
                  });

            return new HackerItemDataResponse()
            {
                Data = results.Take(bestN).ToArray(),
                Status = true
            };
        }

        // Cancellation is not 
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request cancelled");
            return new HackerItemDataResponse
            {
                Data = [],
                Status = false,
                Message = "Request cancelled"
            };
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(GetBestStories));
            return new HackerItemDataResponse()
            {
                Data = [],
                Status = false,
                Message = "Server Error"
            };
        }

    }
}