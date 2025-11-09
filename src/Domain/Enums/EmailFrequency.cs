namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Frequency for batching email notifications.
/// </summary>
public enum EmailFrequency
{
    /// <summary>
    /// Send emails immediately (real-time).
    /// </summary>
    Realtime = 0,

    /// <summary>
    /// Send daily digest at end of day.
    /// </summary>
    Daily = 1,

    /// <summary>
    /// Send weekly digest.
    /// </summary>
    Weekly = 2,

    /// <summary>
    /// Never send email notifications.
    /// </summary>
    Never = 3
}
