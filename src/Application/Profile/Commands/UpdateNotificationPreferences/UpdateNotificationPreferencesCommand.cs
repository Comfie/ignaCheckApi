using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Profile.Commands.UpdateNotificationPreferences;

/// <summary>
/// Command to update notification preferences.
/// </summary>
public record UpdateNotificationPreferencesCommand : IRequest<Result<NotificationPreferencesDto>>
{
    /// <summary>
    /// Email notifications enabled.
    /// </summary>
    public bool? EmailNotifications { get; init; }

    /// <summary>
    /// Task assignment notifications.
    /// </summary>
    public bool? TaskAssignmentNotifications { get; init; }

    /// <summary>
    /// Task deadline notifications.
    /// </summary>
    public bool? TaskDeadlineNotifications { get; init; }

    /// <summary>
    /// Comment notifications.
    /// </summary>
    public bool? CommentNotifications { get; init; }

    /// <summary>
    /// Mention notifications.
    /// </summary>
    public bool? MentionNotifications { get; init; }

    /// <summary>
    /// Workspace invitation notifications.
    /// </summary>
    public bool? WorkspaceInvitationNotifications { get; init; }

    /// <summary>
    /// Weekly digest enabled.
    /// </summary>
    public bool? WeeklyDigest { get; init; }

    /// <summary>
    /// Marketing emails enabled.
    /// </summary>
    public bool? MarketingEmails { get; init; }
}

/// <summary>
/// Notification preferences DTO.
/// </summary>
public record NotificationPreferencesDto
{
    public bool EmailNotifications { get; set; }
    public bool TaskAssignmentNotifications { get; set; }
    public bool TaskDeadlineNotifications { get; set; }
    public bool CommentNotifications { get; set; }
    public bool MentionNotifications { get; set; }
    public bool WorkspaceInvitationNotifications { get; set; }
    public bool WeeklyDigest { get; set; }
    public bool MarketingEmails { get; set; }
}

/// <summary>
/// Handler for the UpdateNotificationPreferencesCommand.
/// </summary>
public class UpdateNotificationPreferencesCommandHandler : IRequestHandler<UpdateNotificationPreferencesCommand, Result<NotificationPreferencesDto>>
{
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;

    public UpdateNotificationPreferencesCommandHandler(
        IUser currentUser,
        IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result<NotificationPreferencesDto>> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<NotificationPreferencesDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get current preferences
        var currentPreferencesJson = await _identityService.GetNotificationPreferencesAsync(_currentUser.Id);
        var preferences = ParsePreferences(currentPreferencesJson);

        // Update preferences if provided
        if (request.EmailNotifications.HasValue)
            preferences.EmailNotifications = request.EmailNotifications.Value;

        if (request.TaskAssignmentNotifications.HasValue)
            preferences.TaskAssignmentNotifications = request.TaskAssignmentNotifications.Value;

        if (request.TaskDeadlineNotifications.HasValue)
            preferences.TaskDeadlineNotifications = request.TaskDeadlineNotifications.Value;

        if (request.CommentNotifications.HasValue)
            preferences.CommentNotifications = request.CommentNotifications.Value;

        if (request.MentionNotifications.HasValue)
            preferences.MentionNotifications = request.MentionNotifications.Value;

        if (request.WorkspaceInvitationNotifications.HasValue)
            preferences.WorkspaceInvitationNotifications = request.WorkspaceInvitationNotifications.Value;

        if (request.WeeklyDigest.HasValue)
            preferences.WeeklyDigest = request.WeeklyDigest.Value;

        if (request.MarketingEmails.HasValue)
            preferences.MarketingEmails = request.MarketingEmails.Value;

        // Serialize and save
        var preferencesJson = SerializePreferences(preferences);
        var updateResult = await _identityService.UpdateNotificationPreferencesAsync(_currentUser.Id, preferencesJson);

        if (!updateResult)
        {
            return Result<NotificationPreferencesDto>.Failure(new[] { "Failed to update notification preferences." });
        }

        return Result<NotificationPreferencesDto>.Success(preferences);
    }

    private static NotificationPreferencesDto ParsePreferences(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            // Return default preferences
            return new NotificationPreferencesDto
            {
                EmailNotifications = true,
                TaskAssignmentNotifications = true,
                TaskDeadlineNotifications = true,
                CommentNotifications = true,
                MentionNotifications = true,
                WorkspaceInvitationNotifications = true,
                WeeklyDigest = true,
                MarketingEmails = false
            };
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<NotificationPreferencesDto>(json)
                ?? new NotificationPreferencesDto();
        }
        catch
        {
            // Return default if deserialization fails
            return new NotificationPreferencesDto
            {
                EmailNotifications = true,
                TaskAssignmentNotifications = true,
                TaskDeadlineNotifications = true,
                CommentNotifications = true,
                MentionNotifications = true,
                WorkspaceInvitationNotifications = true,
                WeeklyDigest = true,
                MarketingEmails = false
            };
        }
    }

    private static string SerializePreferences(NotificationPreferencesDto preferences)
    {
        return System.Text.Json.JsonSerializer.Serialize(preferences);
    }
}
