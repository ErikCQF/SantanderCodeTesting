using SantanderCodeTesting.Model;

namespace SantanderCodeTesting.InfraStructure;

public interface IHackerService
{
    Task<HackerItemDataResponse> GetBestStories(int bestN, CancellationToken cancellationToken);
}
