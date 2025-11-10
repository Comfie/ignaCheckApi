namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Join entity representing the assignment of a compliance framework to a project.
/// A project can be assessed against multiple frameworks simultaneously.
/// </summary>
public class ProjectFramework : BaseAuditableEntity
{
    /// <summary>
    /// The project being assessed.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// The compliance framework to assess against.
    /// </summary>
    public Guid FrameworkId { get; set; }

    /// <summary>
    /// Date when this framework was assigned to the project.
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Target date for achieving compliance.
    /// </summary>
    public DateTime? TargetCompletionDate { get; set; }

    /// <summary>
    /// Date when compliance was achieved (all critical findings resolved).
    /// </summary>
    public DateTime? ComplianceAchievedDate { get; set; }

    /// <summary>
    /// Overall compliance status for this framework on this project.
    /// </summary>
    public ComplianceStatus Status { get; set; } = ComplianceStatus.NotAssessed;

    /// <summary>
    /// Percentage of controls that are compliant (0-100).
    /// Calculated based on findings.
    /// </summary>
    public decimal CompliancePercentage { get; set; }

    /// <summary>
    /// Number of controls marked as compliant.
    /// </summary>
    public int CompliantControlsCount { get; set; }

    /// <summary>
    /// Number of controls marked as partially compliant.
    /// </summary>
    public int PartiallyCompliantControlsCount { get; set; }

    /// <summary>
    /// Number of controls marked as non-compliant.
    /// </summary>
    public int NonCompliantControlsCount { get; set; }

    /// <summary>
    /// Number of controls not yet assessed.
    /// </summary>
    public int NotAssessedControlsCount { get; set; }

    /// <summary>
    /// Total number of controls in this framework.
    /// </summary>
    public int TotalControlsCount { get; set; }

    /// <summary>
    /// Date of the last AI analysis run for this framework.
    /// </summary>
    public DateTime? LastAnalysisDate { get; set; }

    /// <summary>
    /// User or system that performed the last analysis.
    /// </summary>
    public string? LastAnalysisBy { get; set; }

    /// <summary>
    /// Notes specific to this framework assignment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates whether this framework is actively being tracked.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// The project this framework is assigned to.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// The compliance framework.
    /// </summary>
    public ComplianceFramework Framework { get; set; } = null!;
}
