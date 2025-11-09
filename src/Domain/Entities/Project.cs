namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents an audit project/engagement within an organization.
/// This is the main aggregate for organizing compliance work.
/// </summary>
public class Project : BaseAuditableEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public DateTime? TargetDate { get; set; }

    // Foreign keys
    public int OrganizationId { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
}
