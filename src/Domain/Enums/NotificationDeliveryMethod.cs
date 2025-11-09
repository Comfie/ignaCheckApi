namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Methods for delivering notifications.
/// </summary>
public enum NotificationDeliveryMethod
{
    /// <summary>
    /// In-app notification only (shown in notification center).
    /// </summary>
    InApp = 0,

    /// <summary>
    /// Email notification.
    /// </summary>
    Email = 1,

    /// <summary>
    /// Both in-app and email.
    /// </summary>
    Both = 2
}
