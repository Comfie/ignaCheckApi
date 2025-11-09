using IgnaCheck.Application.Findings.Commands.AddFindingComment;
using IgnaCheck.Application.Findings.Commands.AssignFinding;
using IgnaCheck.Application.Findings.Commands.UpdateFindingStatus;
using IgnaCheck.Application.Findings.Queries.GetFindingDetails;
using IgnaCheck.Application.Findings.Queries.GetFindingsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for compliance findings management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FindingsController : ApiControllerBase
{
    private readonly ISender _sender;

    public FindingsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all findings for a project with filtering and sorting.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="frameworkId">Filter by framework (optional)</param>
    /// <param name="status">Filter by compliance status (optional)</param>
    /// <param name="workflowStatus">Filter by workflow status (optional)</param>
    /// <param name="riskLevel">Filter by risk level (optional)</param>
    /// <param name="assignedTo">Filter by assigned user (optional)</param>
    /// <param name="searchTerm">Search by title or description (optional)</param>
    /// <param name="sortBy">Sort field (default: RiskLevel)</param>
    /// <param name="sortDirection">Sort direction (default: desc)</param>
    /// <returns>List of findings</returns>
    [HttpGet("project/{projectId}")]
    [ProducesResponseType(typeof(Result<List<FindingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<List<FindingDto>>>> GetProjectFindings(
        Guid projectId,
        [FromQuery] Guid? frameworkId = null,
        [FromQuery] Domain.Enums.ComplianceStatus? status = null,
        [FromQuery] Domain.Enums.FindingWorkflowStatus? workflowStatus = null,
        [FromQuery] Domain.Enums.RiskLevel? riskLevel = null,
        [FromQuery] string? assignedTo = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string sortBy = "RiskLevel",
        [FromQuery] string sortDirection = "desc")
    {
        var query = new GetFindingsListQuery
        {
            ProjectId = projectId,
            FrameworkId = frameworkId,
            Status = status,
            WorkflowStatus = workflowStatus,
            RiskLevel = riskLevel,
            AssignedTo = assignedTo,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about a specific finding.
    /// </summary>
    /// <param name="id">Finding ID</param>
    /// <returns>Finding details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<FindingDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<FindingDetailsDto>>> GetFindingDetails(Guid id)
    {
        var query = new GetFindingDetailsQuery { FindingId = id };
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a finding's workflow status.
    /// </summary>
    /// <param name="id">Finding ID</param>
    /// <param name="command">Status update details</param>
    /// <returns>Success result</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> UpdateFindingStatus(Guid id, [FromBody] UpdateFindingStatusCommand command)
    {
        if (id != command.FindingId)
        {
            return BadRequest(Result.Failure(new[] { "Finding ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Assign a finding to a team member.
    /// </summary>
    /// <param name="id">Finding ID</param>
    /// <param name="command">Assignment details</param>
    /// <returns>Success result</returns>
    [HttpPut("{id}/assign")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> AssignFinding(Guid id, [FromBody] AssignFindingCommand command)
    {
        if (id != command.FindingId)
        {
            return BadRequest(Result.Failure(new[] { "Finding ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add a comment to a finding.
    /// </summary>
    /// <param name="id">Finding ID</param>
    /// <param name="command">Comment details</param>
    /// <returns>Created comment ID</returns>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<Guid>>> AddComment(Guid id, [FromBody] AddFindingCommentCommand command)
    {
        if (id != command.FindingId)
        {
            return BadRequest(Result<Guid>.Failure(new[] { "Finding ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
