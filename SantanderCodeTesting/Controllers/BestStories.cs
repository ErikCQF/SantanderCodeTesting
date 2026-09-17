using Microsoft.AspNetCore.Mvc;
using SantanderCodeTesting.InfraStructure;
using SantanderCodeTesting.Model;

namespace SantanderCodeTesting.Controllers;

[ApiController]
[Route("[controller]")]
public class BestStories : ControllerBase
{

    private readonly ILogger<BestStories> _logger;
    private readonly IHackerService _hackerService;

    public BestStories(ILogger<BestStories> logger,
                      IHackerService hackerService)
    {
        _logger = logger;
        _hackerService = hackerService;
    }

    /// <summary>
    ///   return an array of the best n stories as returned by the Hacker News API in descending order of score
    /// </summary>
    /// <param name="n"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> return an array of the best n stories as returned by the Hacker News API in descending order of score </returns>
    [HttpGet("best")]
    [ProducesResponseType(typeof(HackerItemDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   
    public async Task<IActionResult> GetBest([FromQuery] int n, CancellationToken cancellationToken)
    {
        if (n <= 0)
        {
            return Problem(
                title: "Invalid value for 'n'",
                detail: "'n' must be a positive integer.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var response = await _hackerService.GetBestStories(n, cancellationToken);

        if (!response.Status)
        {
            _logger.LogError("GetBestN failed for n={N}: {Message}", n, response.Message);
            return Problem(
                title: "Upstream error",
                detail: response.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(response.Data);
    }
}


