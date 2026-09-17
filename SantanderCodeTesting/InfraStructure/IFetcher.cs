using SantanderCodeTesting.Model;

namespace SantanderCodeTesting.InfraStructure;

public interface IFetcher
{
    Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken);
    Task<HackerItemData?> GetItemAsync(int id, CancellationToken cancellationToken);
}
