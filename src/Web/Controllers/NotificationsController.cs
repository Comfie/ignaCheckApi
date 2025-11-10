using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Notifications.Commands.MarkNotificationsAsRead;
using IgnaCheck.Application.Notifications.Commands.UpdateNotificationPreferences;
using IgnaCheck.Application.Notifications.Queries.GetNotificationPreferences;
using IgnaCheck.Application.Notifications.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for user notifications and preferences.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get user's notifications with optional filtering.
    /// </summary>
    /// <param name="type">Filter by notification type (optional)</param>
    /// <param name="isRead">Filter by read status (optional)</param>
    /// <param name="limit">Maximum number of notifications to return (default: 50, max: 200)</param>
    /// <param name="offset">Offset for pagination (default: 0)</param>
    /// <returns>List of notifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<List<NotificationDto>>>> GetNotifications(
        [FromQuery] Domain.Enums.NotificationType? type = null,
        [FromQuery] bool? isRead = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        var query = new GetNotificationsQuery
        {
            Type = type,
            IsRead = isRead,
            Limit = limit,
            Offset = offset
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mark notifications as read.
    /// </summary>
    /// <param name="command">Mark as read command (empty list marks all unread as read)</param>
    /// <returns>Success result</returns>
    [HttpPost("mark-as-read")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result>> MarkAsRead([FromBody] MarkNotificationsAsReadCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get user's notification preferences.
    /// </summary>
    /// <returns>List of notification preferences for all notification types</returns>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(Result<List<NotificationPreferenceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<List<NotificationPreferenceDto>>>> GetPreferences()
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
    /// Update user's notification preferences.
    /// </summary>
    /// <param name="command">Notification preferences to update</param>
    /// <returns>Success result</returns>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result>> UpdatePreferences([FromBody] UpdateNotificationPreferencesCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
