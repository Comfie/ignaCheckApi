using IgnaCheck.Application.Users.Commands.AcceptInvitation;
using IgnaCheck.Application.Users.Commands.DeclineInvitation;
using IgnaCheck.Application.Users.Commands.InviteUser;
using IgnaCheck.Application.Users.Commands.RemoveMember;
using IgnaCheck.Application.Users.Commands.RevokeInvitation;
using IgnaCheck.Application.Users.Commands.UpdateMemberRole;
using IgnaCheck.Application.Users.Queries.GetMyInvitations;
using IgnaCheck.Application.Users.Queries.GetWorkspaceInvitations;
using IgnaCheck.Application.Users.Queries.GetWorkspaceMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for user management operations.
/// </summary>
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get workspace members with filtering and pagination.
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>Paginated list of workspace members</returns>
    [HttpGet("workspace/members")]
    [ProducesResponseType(typeof(Result<WorkspaceMembersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<WorkspaceMembersResponse>>> GetWorkspaceMembers([FromQuery] GetWorkspaceMembersQuery query)
    {
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Invite a user to the workspace.
    /// </summary>
    /// <param name="command">Invitation details</param>
    /// <returns>Invitation details</returns>
    [HttpPost("invite")]
    [ProducesResponseType(typeof(Result<InviteUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<InviteUserResponse>>> InviteUser([FromBody] InviteUserCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all invitations for the workspace.
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <returns>List of invitations</returns>
    [HttpGet("workspace/invitations")]
    [ProducesResponseType(typeof(Result<List<InvitationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<List<InvitationDto>>>> GetWorkspaceInvitations([FromQuery] GetWorkspaceInvitationsQuery query)
    {
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get pending invitations for the current user.
    /// </summary>
    /// <returns>List of pending invitations</returns>
    [HttpGet("my-invitations")]
    [ProducesResponseType(typeof(Result<List<MyInvitationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<List<MyInvitationDto>>>> GetMyInvitations()
    {
        var query = new GetMyInvitationsQuery();
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Accept a workspace invitation.
    /// </summary>
    /// <param name="command">Invitation token</param>
    /// <returns>Workspace details with JWT token</returns>
    [HttpPost("invitations/accept")]
    [ProducesResponseType(typeof(Result<AcceptInvitationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<AcceptInvitationResponse>>> AcceptInvitation([FromBody] AcceptInvitationCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Decline a workspace invitation.
    /// </summary>
    /// <param name="command">Invitation token</param>
    /// <returns>Success result</returns>
    [HttpPost("invitations/decline")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result>> DeclineInvitation([FromBody] DeclineInvitationCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Revoke a pending invitation.
    /// </summary>
    /// <param name="command">Invitation ID</param>
    /// <returns>Success result</returns>
    [HttpPost("invitations/revoke")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> RevokeInvitation([FromBody] RevokeInvitationCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a workspace member's role.
    /// </summary>
    /// <param name="command">User ID and new role</param>
    /// <returns>Success result</returns>
    [HttpPut("workspace/members/role")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> UpdateMemberRole([FromBody] UpdateMemberRoleCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove a member from the workspace.
    /// </summary>
    /// <param name="command">User ID to remove</param>
    /// <returns>Success result</returns>
    [HttpDelete("workspace/members")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> RemoveMember([FromBody] RemoveMemberCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
