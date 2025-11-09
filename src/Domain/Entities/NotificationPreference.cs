namespace IgnaCheck.Domain.Entities;

/// <summary>
/// User preferences for notification delivery.
/// </summary>
public class NotificationPreference : BaseAuditableEntity
{
    /// <summary>
    /// User ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of notification this preference applies to.
    /// </summary>
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// Preferred delivery method for this notification type.
    /// </summary>
    public NotificationDeliveryMethod DeliveryMethod { get; set; } = NotificationDeliveryMethod.Both;

    /// <summary>
    /// Email frequency preference (realtime, daily, weekly, never).
    /// </summary>
    public EmailFrequency EmailFrequency { get; set; } = EmailFrequency.Realtime;

    /// <summary>
    /// Indicates whether this notification type is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
