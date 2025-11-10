using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Infrastructure.Services;

/// <summary>
/// Service for creating and sending notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        IEmailService emailService,
        IIdentityService identityService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task SendNotificationAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        string? link = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken cancellationToken = default)
    {
        await SendNotificationAsync(
            new List<string> { userId },
            type,
            title,
            message,
            link,
            relatedEntityId,
            relatedEntityType,
            cancellationToken);
    }

    public async Task SendNotificationAsync(
        List<string> userIds,
        NotificationType type,
        string title,
        string message,
        string? link = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            try
            {
                // Get user's notification preferences
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == type, cancellationToken);

                // If no preference exists, use defaults (all notifications enabled with realtime email)
                var isEnabled = preference?.IsEnabled ?? true;
                var deliveryMethod = preference?.DeliveryMethod ?? NotificationDeliveryMethod.Both;
                var emailFrequency = preference?.EmailFrequency ?? EmailFrequency.Realtime;

                // Skip if notifications are disabled for this type
                if (!isEnabled)
                {
                    _logger.LogInformation("Notification type {Type} is disabled for user {UserId}", type, userId);
                    continue;
                }

                // Create in-app notification if delivery method includes in-app
                if (deliveryMethod == NotificationDeliveryMethod.InApp || deliveryMethod == NotificationDeliveryMethod.Both)
                {
                    var notification = new Notification
                    {
                        UserId = userId,
                        Type = type,
                        Title = title,
                        Message = message,
                        Link = link,
                        RelatedEntityId = relatedEntityId,
                        RelatedEntityType = relatedEntityType,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);
                }

                // Send email if delivery method includes email and frequency is realtime
                if ((deliveryMethod == NotificationDeliveryMethod.Email || deliveryMethod == NotificationDeliveryMethod.Both) &&
                    emailFrequency == EmailFrequency.Realtime)
                {
                    // Get user email using identity service
                    var userObj = await _identityService.GetUserByIdAsync(userId);
                    if (userObj is Infrastructure.Identity.ApplicationUser user && !string.IsNullOrWhiteSpace(user.Email))
                    {
                        try
                        {
                            // Send email using the generic email service
                            await _emailService.SendEmailAsync(
                                user.Email,
                                title,
                                GenerateHtmlEmailBody(title, message, link),
                                message, // Plain text fallback
                                cancellationToken);

                            _logger.LogInformation("Email sent successfully to {Email} for notification type {Type}", user.Email, type);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send email to {Email} for notification type {Type}", user.Email, type);
                            // Don't throw - we want to continue processing other notifications
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId} for type {Type}", userId, type);
                // Continue processing other users
            }
        }
    }

    private static string GenerateHtmlEmailBody(string title, string message, string? link)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>IgnaCheck.ai</h1>
        </div>
        <div class=""content"">
            <h2>{title}</h2>
            <p>{message}</p>
            {(link != null ? $@"<a href=""{link}"" class=""button"">View Details</a>" : "")}
        </div>
        <div class=""footer"">
            <p>© 2024 IgnaCheck.ai. All rights reserved.</p>
            <p>You're receiving this email because you have notifications enabled for this activity.</p>
        </div>
    </div>
</body>
</html>";

        return html;
    }
}
