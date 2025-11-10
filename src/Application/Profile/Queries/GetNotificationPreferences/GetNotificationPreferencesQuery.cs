using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Profile.Commands.UpdateNotificationPreferences;

namespace IgnaCheck.Application.Profile.Queries.GetNotificationPreferences;

/// <summary>
/// Query to get the current user's notification preferences.
/// </summary>
public record GetNotificationPreferencesQuery : IRequest<Result<NotificationPreferencesDto>>
{
}

/// <summary>
/// Handler for the GetNotificationPreferencesQuery.
/// </summary>
public class GetNotificationPreferencesQueryHandler : IRequestHandler<GetNotificationPreferencesQuery, Result<NotificationPreferencesDto>>
{
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;

    public GetNotificationPreferencesQueryHandler(
        IUser currentUser,
        IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result<NotificationPreferencesDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<NotificationPreferencesDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get preferences from identity service
        var preferencesJson = await _identityService.GetNotificationPreferencesAsync(_currentUser.Id);
        var preferences = ParsePreferences(preferencesJson);

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
}
