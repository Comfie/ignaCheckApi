using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Notifications.Queries.GetNotificationPreferences;

/// <summary>
/// Query to get user's notification preferences.
/// </summary>
public record GetNotificationPreferencesQuery : IRequest<Result<List<NotificationPreferenceDto>>>;

/// <summary>
/// Notification preference DTO.
/// </summary>
public record NotificationPreferenceDto
{
    public NotificationType NotificationType { get; init; }
    public NotificationDeliveryMethod DeliveryMethod { get; init; }
    public EmailFrequency EmailFrequency { get; init; }
    public bool IsEnabled { get; init; }
}

/// <summary>
/// Handler for GetNotificationPreferencesQuery.
/// </summary>
public class GetNotificationPreferencesQueryHandler : IRequestHandler<GetNotificationPreferencesQuery, Result<List<NotificationPreferenceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetNotificationPreferencesQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NotificationPreferenceDto>>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<NotificationPreferenceDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Get user's preferences
        var preferences = await _context.NotificationPreferences
            .Where(p => p.UserId == _currentUser.Id)
            .Select(p => new NotificationPreferenceDto
            {
                NotificationType = p.NotificationType,
                DeliveryMethod = p.DeliveryMethod,
                EmailFrequency = p.EmailFrequency,
                IsEnabled = p.IsEnabled
            })
            .ToListAsync(cancellationToken);

        // If no preferences exist, return default preferences for all notification types
        if (!preferences.Any())
        {
            preferences = Enum.GetValues<NotificationType>()
                .Select(type => new NotificationPreferenceDto
                {
                    NotificationType = type,
                    DeliveryMethod = NotificationDeliveryMethod.Both,
                    EmailFrequency = EmailFrequency.Realtime,
                    IsEnabled = true
                })
                .ToList();
        }
        else
        {
            // Fill in missing notification types with defaults
            var existingTypes = preferences.Select(p => p.NotificationType).ToHashSet();
            var missingTypes = Enum.GetValues<NotificationType>()
                .Where(type => !existingTypes.Contains(type))
                .Select(type => new NotificationPreferenceDto
                {
                    NotificationType = type,
                    DeliveryMethod = NotificationDeliveryMethod.Both,
                    EmailFrequency = EmailFrequency.Realtime,
                    IsEnabled = true
                });

            preferences.AddRange(missingTypes);
        }

        return Result<List<NotificationPreferenceDto>>.Success(preferences.OrderBy(p => p.NotificationType).ToList());
    }
}
