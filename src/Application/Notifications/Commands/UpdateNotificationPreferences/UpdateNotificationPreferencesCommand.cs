using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Notifications.Commands.UpdateNotificationPreferences;

/// <summary>
/// Command to update user's notification preferences.
/// </summary>
public record UpdateNotificationPreferencesCommand : IRequest<Result>
{
    /// <summary>
    /// List of notification preferences to update.
    /// </summary>
    public List<NotificationPreferenceUpdate> Preferences { get; init; } = new();
}

public record NotificationPreferenceUpdate
{
    public NotificationType NotificationType { get; init; }
    public NotificationDeliveryMethod DeliveryMethod { get; init; }
    public EmailFrequency EmailFrequency { get; init; }
    public bool IsEnabled { get; init; }
}

/// <summary>
/// Validator for UpdateNotificationPreferencesCommand.
/// </summary>
public class UpdateNotificationPreferencesCommandValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesCommandValidator()
    {
        RuleFor(v => v.Preferences)
            .NotEmpty().WithMessage("At least one preference must be provided.");

        RuleForEach(v => v.Preferences).ChildRules(preference =>
        {
            preference.RuleFor(p => p.NotificationType)
                .IsInEnum().WithMessage("Invalid notification type.");

            preference.RuleFor(p => p.DeliveryMethod)
                .IsInEnum().WithMessage("Invalid delivery method.");

            preference.RuleFor(p => p.EmailFrequency)
                .IsInEnum().WithMessage("Invalid email frequency.");
        });
    }
}

/// <summary>
/// Handler for UpdateNotificationPreferencesCommand.
/// </summary>
public class UpdateNotificationPreferencesCommandHandler : IRequestHandler<UpdateNotificationPreferencesCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateNotificationPreferencesCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Get existing preferences
        var existingPreferences = await _context.NotificationPreferences
            .Where(p => p.UserId == _currentUser.Id)
            .ToListAsync(cancellationToken);

        // Update or create preferences
        foreach (var preferenceUpdate in request.Preferences)
        {
            var existing = existingPreferences.FirstOrDefault(p => p.NotificationType == preferenceUpdate.NotificationType);

            if (existing != null)
            {
                // Update existing preference
                existing.DeliveryMethod = preferenceUpdate.DeliveryMethod;
                existing.EmailFrequency = preferenceUpdate.EmailFrequency;
                existing.IsEnabled = preferenceUpdate.IsEnabled;
            }
            else
            {
                // Create new preference
                var newPreference = new NotificationPreference
                {
                    UserId = _currentUser.Id!,
                    NotificationType = preferenceUpdate.NotificationType,
                    DeliveryMethod = preferenceUpdate.DeliveryMethod,
                    EmailFrequency = preferenceUpdate.EmailFrequency,
                    IsEnabled = preferenceUpdate.IsEnabled
                };

                _context.NotificationPreferences.Add(newPreference);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
