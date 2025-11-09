using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for creating and sending notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification to a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">Notification type</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="link">Optional link to related entity</param>
    /// <param name="relatedEntityId">Optional related entity ID</param>
    /// <param name="relatedEntityType">Optional related entity type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendNotificationAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        string? link = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a notification to multiple users.
    /// </summary>
    /// <param name="userIds">List of user IDs</param>
    /// <param name="type">Notification type</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="link">Optional link to related entity</param>
    /// <param name="relatedEntityId">Optional related entity ID</param>
    /// <param name="relatedEntityType">Optional related entity type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendNotificationAsync(
        List<string> userIds,
        NotificationType type,
        string title,
        string message,
        string? link = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken cancellationToken = default);
}
