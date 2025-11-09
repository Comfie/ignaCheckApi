namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents an individual control or requirement within a compliance framework.
/// For example, "A.5.1 Policies for information security" in ISO 27001.
/// </summary>
public class ComplianceControl : BaseAuditableEntity
{
    /// <summary>
    /// The compliance framework this control belongs to.
    /// </summary>
    public Guid FrameworkId { get; set; }

    /// <summary>
    /// Control identifier within the framework (e.g., "A.5.1", "CC6.1", "Article 5").
    /// </summary>
    public string ControlCode { get; set; } = string.Empty;

    /// <summary>
    /// The title or name of the control.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of what the control requires.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Implementation guidance or recommendations.
    /// </summary>
    public string? ImplementationGuidance { get; set; }

    /// <summary>
    /// Category or domain within the framework (e.g., "Access Control", "Cryptography").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Sub-category for more granular organization.
    /// </summary>
    public string? SubCategory { get; set; }

    /// <summary>
    /// Parent control ID if this is a sub-control (hierarchical structure).
    /// </summary>
    public Guid? ParentControlId { get; set; }

    /// <summary>
    /// Risk level if this control is not implemented.
    /// </summary>
    public RiskLevel DefaultRiskLevel { get; set; } = RiskLevel.Medium;

    /// <summary>
    /// Display order within the framework.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indicates whether this control is mandatory or optional.
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Tags for categorization and search (e.g., "encryption", "access-control").
    /// Stored as JSON array.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Reference URLs to official documentation or guidance.
    /// Stored as JSON array of URLs.
    /// </summary>
    public string? ReferenceUrls { get; set; }

    /// <summary>
    /// Example evidence types that satisfy this control.
    /// Stored as JSON array (e.g., ["Policy Document", "Audit Log", "Configuration Screenshot"]).
    /// </summary>
    public string? ExampleEvidenceTypes { get; set; }

    // Navigation properties

    /// <summary>
    /// The framework this control belongs to.
    /// </summary>
    public ComplianceFramework Framework { get; set; } = null!;

    /// <summary>
    /// Parent control if this is a sub-control.
    /// </summary>
    public ComplianceControl? ParentControl { get; set; }

    /// <summary>
    /// Sub-controls under this control.
    /// </summary>
    public ICollection<ComplianceControl> SubControls { get; set; } = new List<ComplianceControl>();

    /// <summary>
    /// Findings related to this control across projects.
    /// </summary>
    public ICollection<ComplianceFinding> Findings { get; set; } = new List<ComplianceFinding>();
}
