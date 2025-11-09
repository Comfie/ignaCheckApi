using IgnaCheck.Application.Profile.Commands.UpdateAvatar;
using IgnaCheck.Application.Profile.Commands.UpdateMyProfile;
using IgnaCheck.Application.Profile.Commands.UpdateNotificationPreferences;
using IgnaCheck.Application.Profile.Queries.GetMyProfile;
using IgnaCheck.Application.Profile.Queries.GetNotificationPreferences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for user profile management operations.
/// </summary>
[Authorize]
public class ProfileController : ApiControllerBase
{
    private readonly ISender _sender;

    public ProfileController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get the current user's profile.
    /// </summary>
    /// <returns>User profile</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<UserProfileDto>>> GetMyProfile()
    {
        var query = new GetMyProfileQuery();
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update the current user's profile.
    /// </summary>
    /// <param name="command">Profile update details</param>
    /// <returns>Updated profile</returns>
    [HttpPut]
    [ProducesResponseType(typeof(Result<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<UserProfileDto>>> UpdateMyProfile([FromBody] UpdateMyProfileCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update the current user's avatar.
    /// </summary>
    /// <param name="command">Avatar URL</param>
    /// <returns>Updated avatar URL</returns>
    [HttpPut("avatar")]
    [ProducesResponseType(typeof(Result<UpdateAvatarResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<UpdateAvatarResponse>>> UpdateAvatar([FromBody] UpdateAvatarCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get the current user's notification preferences.
    /// </summary>
    /// <returns>Notification preferences</returns>
    [HttpGet("notification-preferences")]
    [ProducesResponseType(typeof(Result<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<NotificationPreferencesDto>>> GetNotificationPreferences()
    {
        var query = new GetNotificationPreferencesQuery();
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update the current user's notification preferences.
    /// </summary>
    /// <param name="command">Notification preferences</param>
    /// <returns>Updated preferences</returns>
    [HttpPut("notification-preferences")]
    [ProducesResponseType(typeof(Result<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<NotificationPreferencesDto>>> UpdateNotificationPreferences([FromBody] UpdateNotificationPreferencesCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
