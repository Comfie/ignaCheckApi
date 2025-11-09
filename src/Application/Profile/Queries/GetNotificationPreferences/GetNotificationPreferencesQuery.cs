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
    private readonly IApplicationDbContext _context;

    public GetNotificationPreferencesQueryHandler(
        IUser currentUser,
        IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<Result<NotificationPreferencesDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<NotificationPreferencesDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get user from database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);
        if (user == null)
        {
            return Result<NotificationPreferencesDto>.Failure(new[] { "User not found." });
        }

        // Parse preferences
        var preferences = ParsePreferences(user.NotificationPreferences);

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
