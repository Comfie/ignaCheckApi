namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a tenant organization in the multi-tenant system.
/// This is the root aggregate for multi-tenancy isolation.
/// </summary>
public class Organization : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Domain { get; set; }

    public string? SubscriptionTier { get; set; }

    public DateTime? SubscriptionExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Settings { get; set; } // JSON settings

    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
