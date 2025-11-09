namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Type of evidence relationship to a finding.
/// </summary>
public enum EvidenceType
{
    /// <summary>
    /// Document supports compliance (shows the control is met).
    /// </summary>
    Supporting = 0,

    /// <summary>
    /// Document shows a gap or non-compliance.
    /// </summary>
    Contradicting = 1,

    /// <summary>
    /// Document is relevant but neither clearly supports nor contradicts.
    /// </summary>
    Contextual = 2,

    /// <summary>
    /// Document was used for remediation/resolution.
    /// </summary>
    Remediation = 3
}
