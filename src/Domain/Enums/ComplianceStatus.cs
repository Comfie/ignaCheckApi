namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Represents the compliance status of a control or requirement.
/// </summary>
public enum ComplianceStatus
{
    NotAssessed = 0,
    Compliant = 1,
    PartiallyCompliant = 2,
    NonCompliant = 3,
    NotApplicable = 4
}
