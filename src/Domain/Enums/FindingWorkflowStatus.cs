namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Represents the workflow status for tracking finding remediation progress.
/// </summary>
public enum FindingWorkflowStatus
{
    /// <summary>
    /// Finding has been identified but work has not started.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Remediation work is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Finding has been remediated and resolved.
    /// </summary>
    Resolved = 2,

    /// <summary>
    /// Finding has been accepted as a known risk (will not remediate).
    /// </summary>
    Accepted = 3,

    /// <summary>
    /// Finding was determined to be a false positive.
    /// </summary>
    FalsePositive = 4
}
