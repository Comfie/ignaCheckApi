using IgnaCheck.Application.Frameworks.Commands.RemoveProjectFramework;
using IgnaCheck.Application.Frameworks.Commands.SelectProjectFrameworks;
using IgnaCheck.Application.Frameworks.Queries.GetFrameworkDetails;
using IgnaCheck.Application.Frameworks.Queries.GetFrameworks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for compliance framework operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FrameworksController : ApiControllerBase
{
    private readonly ISender _sender;

    public FrameworksController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all available compliance frameworks.
    /// </summary>
    /// <param name="category">Filter by framework category (optional)</param>
    /// <param name="activeOnly">Show only active frameworks (default: true)</param>
    /// <param name="includeControlCount">Include control count (default: true)</param>
    /// <returns>List of available frameworks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<FrameworkDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<List<FrameworkDto>>>> GetFrameworks(
        [FromQuery] Domain.Enums.FrameworkCategory? category = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] bool includeControlCount = true)
    {
        var query = new GetFrameworksQuery
        {
            Category = category,
            ActiveOnly = activeOnly,
            IncludeControlCount = includeControlCount
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about a specific compliance framework including all its controls.
    /// </summary>
    /// <param name="id">Framework ID</param>
    /// <param name="includeControls">Include full control details (default: true)</param>
    /// <returns>Framework details with controls</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<FrameworkDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<FrameworkDetailsDto>>> GetFrameworkDetails(
        Guid id,
        [FromQuery] bool includeControls = true)
    {
        var query = new GetFrameworkDetailsQuery
        {
            FrameworkId = id,
            IncludeControls = includeControls
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Assign/select compliance frameworks to a project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="command">Framework selection details</param>
    /// <returns>Success or failure result</returns>
    [HttpPost("projects/{projectId}/frameworks")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> SelectProjectFrameworks(
        Guid projectId,
        [FromBody] SelectProjectFrameworksRequest request)
    {
        var command = new SelectProjectFrameworksCommand
        {
            ProjectId = projectId,
            FrameworkIds = request.FrameworkIds,
            TargetCompletionDate = request.TargetCompletionDate,
            Notes = request.Notes
        };

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove a compliance framework from a project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="frameworkId">Framework ID to remove</param>
    /// <returns>Success or failure result</returns>
    [HttpDelete("projects/{projectId}/frameworks/{frameworkId}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> RemoveProjectFramework(Guid projectId, Guid frameworkId)
    {
        var command = new RemoveProjectFrameworkCommand
        {
            ProjectId = projectId,
            FrameworkId = frameworkId
        };

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

/// <summary>
/// Request model for selecting project frameworks.
/// </summary>
public record SelectProjectFrameworksRequest
{
    /// <summary>
    /// Framework IDs to assign to the project.
    /// </summary>
    public List<Guid> FrameworkIds { get; init; } = new();

    /// <summary>
    /// Optional target completion date for all frameworks.
    /// </summary>
    public DateTime? TargetCompletionDate { get; init; }

    /// <summary>
    /// Optional notes for the framework assignment.
    /// </summary>
    public string? Notes { get; init; }
}
