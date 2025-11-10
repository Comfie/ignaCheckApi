using IgnaCheck.Application.Audit.Commands.RunAuditCheck;
using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Common.Models.AI;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for audit and compliance check operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuditController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Run an AI-powered compliance audit check for a project against a framework.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="frameworkId">Framework ID to audit against</param>
    /// <param name="request">Analysis options</param>
    /// <returns>Audit check result with findings</returns>
    [HttpPost("projects/{projectId}/frameworks/{frameworkId}/run")]
    [ProducesResponseType(typeof(Result<AuditCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<AuditCheckResponse>>> RunAuditCheck(
        Guid projectId,
        Guid frameworkId,
        [FromBody] RunAuditCheckRequest? request = null)
    {
        var command = new RunAuditCheckCommand
        {
            ProjectId = projectId,
            FrameworkId = frameworkId,
            Options = request?.Options
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
/// Request model for running an audit check.
/// </summary>
public record RunAuditCheckRequest
{
    /// <summary>
    /// Optional analysis options.
    /// </summary>
    public AnalysisOptions? Options { get; init; }
}
