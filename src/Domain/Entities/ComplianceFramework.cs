namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a regulatory compliance framework (e.g., ISO 27001, SOC 2, GDPR, PCI DSS).
/// This is an aggregate root that contains compliance controls and requirements.
/// Can be system-wide (OrganizationId = null) or tenant-specific (OrganizationId = value).
/// </summary>
public class ComplianceFramework : BaseAuditableEntity
{
    /// <summary>
    /// Organization (tenant) that owns this framework customization.
    /// Null for system-provided default frameworks.
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// The framework identifier (e.g., "ISO27001", "SOC2-Type2", "GDPR").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the framework (e.g., "ISO/IEC 27001:2022").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the framework and its purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The version of the framework (e.g., "2022", "2.0").
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Category classification of the framework.
    /// </summary>
    public FrameworkCategory Category { get; set; }

    /// <summary>
    /// Issuing organization or authority (e.g., "ISO/IEC", "AICPA", "EU Parliament").
    /// </summary>
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// Date when this framework version was published.
    /// </summary>
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Date when this framework version becomes effective.
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Indicates whether this is a system-provided framework (read-only for tenants).
    /// System frameworks can be cloned and customized by organizations.
    /// </summary>
    public bool IsSystemFramework { get; set; }

    /// <summary>
    /// If this is a customized copy, references the original system framework.
    /// </summary>
    public Guid? ParentFrameworkId { get; set; }

    /// <summary>
    /// Indicates whether this framework is currently active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Compliance controls that belong to this framework.
    /// </summary>
    public ICollection<ComplianceControl> Controls { get; set; } = new List<ComplianceControl>();

    /// <summary>
    /// Projects that are assessed against this framework.
    /// </summary>
    public ICollection<ProjectFramework> ProjectFrameworks { get; set; } = new List<ProjectFramework>();

    /// <summary>
    /// Reference to parent framework if this is a customized version.
    /// </summary>
    public ComplianceFramework? ParentFramework { get; set; }

    /// <summary>
    /// Customized copies of this framework (if this is a system framework).
    /// </summary>
    public ICollection<ComplianceFramework> CustomizedVersions { get; set; } = new List<ComplianceFramework>();
}
