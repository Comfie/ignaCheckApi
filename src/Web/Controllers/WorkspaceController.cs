using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Workspaces.Commands.CreateWorkspace;
using IgnaCheck.Application.Workspaces.Commands.SwitchWorkspace;
using IgnaCheck.Application.Workspaces.Commands.UpdateWorkspaceSettings;
using IgnaCheck.Application.Workspaces.Queries.GetMyWorkspaces;
using IgnaCheck.Application.Workspaces.Queries.GetWorkspaceSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for workspace management operations.
/// </summary>
[Authorize]
public class WorkspaceController : ApiControllerBase
{
    private readonly ISender _sender;

    public WorkspaceController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all workspaces the current user belongs to.
    /// </summary>
    /// <returns>List of workspaces</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<WorkspaceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<List<WorkspaceDto>>>> GetMyWorkspaces()
    {
        var query = new GetMyWorkspacesQuery();
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new workspace.
    /// </summary>
    /// <param name="command">Workspace creation details</param>
    /// <returns>Created workspace details with JWT token</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CreateWorkspaceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<CreateWorkspaceResponse>>> CreateWorkspace([FromBody] CreateWorkspaceCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Switch to a different workspace.
    /// </summary>
    /// <param name="command">Workspace switch details</param>
    /// <returns>New JWT token with updated workspace context</returns>
    [HttpPost("switch")]
    [ProducesResponseType(typeof(Result<SwitchWorkspaceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<SwitchWorkspaceResponse>>> SwitchWorkspace([FromBody] SwitchWorkspaceCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get workspace settings.
    /// </summary>
    /// <param name="workspaceId">Workspace ID (optional - uses current workspace if not provided)</param>
    /// <returns>Workspace settings</returns>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(Result<WorkspaceSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<WorkspaceSettingsDto>>> GetWorkspaceSettings([FromQuery] Guid? workspaceId = null)
    {
        var query = new GetWorkspaceSettingsQuery { WorkspaceId = workspaceId };
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update workspace settings.
    /// </summary>
    /// <param name="command">Updated workspace settings</param>
    /// <returns>Updated workspace settings</returns>
    [HttpPut("settings")]
    [ProducesResponseType(typeof(Result<WorkspaceSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<WorkspaceSettingsDto>>> UpdateWorkspaceSettings([FromBody] UpdateWorkspaceSettingsCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
