namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a piece of evidence (document) linked to a compliance finding.
/// This can be supporting evidence (shows compliance) or contradicting evidence (shows gap).
/// </summary>
public class FindingEvidence : BaseAuditableEntity
{
    /// <summary>
    /// The finding this evidence relates to.
    /// </summary>
    public Guid FindingId { get; set; }

    /// <summary>
    /// The document serving as evidence.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Type of evidence relationship.
    /// </summary>
    public EvidenceType EvidenceType { get; set; }

    /// <summary>
    /// Relevant excerpt or quote from the document.
    /// </summary>
    public string? Excerpt { get; set; }

    /// <summary>
    /// Page number or section reference within the document.
    /// </summary>
    public string? PageReference { get; set; }

    /// <summary>
    /// AI confidence that this document is relevant (0.0 - 1.0).
    /// </summary>
    public decimal? RelevanceScore { get; set; }

    /// <summary>
    /// Notes about how this evidence supports or contradicts the finding.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates whether this evidence was manually added or AI-detected.
    /// </summary>
    public bool IsManuallyAdded { get; set; }

    // Navigation properties

    /// <summary>
    /// The compliance finding.
    /// </summary>
    public ComplianceFinding Finding { get; set; } = null!;

    /// <summary>
    /// The evidence document.
    /// </summary>
    public Document Document { get; set; } = null!;
}
