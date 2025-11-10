using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Search.Queries.GlobalSearch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for global search functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ApiControllerBase
{
    private readonly ISender _sender;

    public SearchController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Global search across workspace (projects, documents, findings, tasks).
    /// </summary>
    /// <param name="searchTerm">Search term (minimum 2 characters)</param>
    /// <param name="resultTypes">Filter by result types (optional - comma-separated: Project,Document,Finding,Task)</param>
    /// <param name="projectId">Filter by project ID (optional)</param>
    /// <param name="maxResultsPerType">Maximum results per type (default: 10, max: 50)</param>
    /// <returns>Search results grouped by type</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<GlobalSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<GlobalSearchResultDto>>> GlobalSearch(
        [FromQuery] string searchTerm,
        [FromQuery] string? resultTypes = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] int maxResultsPerType = 10)
    {
        // Parse result types if provided
        List<SearchResultType>? parsedResultTypes = null;
        if (!string.IsNullOrWhiteSpace(resultTypes))
        {
            parsedResultTypes = resultTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Enum.TryParse<SearchResultType>(t.Trim(), true, out var type) ? type : (SearchResultType?)null)
                .Where(t => t.HasValue)
                .Select(t => t!.Value)
                .ToList();
        }

        var query = new GlobalSearchQuery
        {
            SearchTerm = searchTerm,
            ResultTypes = parsedResultTypes,
            ProjectId = projectId,
            MaxResultsPerType = maxResultsPerType
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
