namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a tenant organization (workspace) in the multi-tenant system.
/// This is the root aggregate for multi-tenancy isolation.
/// Each organization has its own set of users, projects, and data.
/// </summary>
public class Organization : BaseAuditableEntity
{
    /// <summary>
    /// The workspace/organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug for the organization (e.g., "acme-corp").
    /// Used in workspace URLs: app.ignacheck.ai/acme-corp
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Company domain name (e.g., "acme.com").
    /// Used for email domain verification and single sign-on.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Organization logo URL or path.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Organization description or notes.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Industry or sector (e.g., "Financial Services", "Healthcare").
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Organization size category (e.g., "1-10", "11-50", "51-200").
    /// </summary>
    public string? CompanySize { get; set; }

    /// <summary>
    /// Primary contact email for the organization.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Primary contact phone number.
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Subscription plan tier (e.g., "Free", "Professional", "Enterprise").
    /// </summary>
    public string? SubscriptionTier { get; set; }

    /// <summary>
    /// Date when the subscription expires.
    /// </summary>
    public DateTime? SubscriptionExpiresAt { get; set; }

    /// <summary>
    /// Date when the trial period ends (if applicable).
    /// </summary>
    public DateTime? TrialEndsAt { get; set; }

    /// <summary>
    /// Indicates whether this organization is on a trial.
    /// </summary>
    public bool IsTrialActive => TrialEndsAt.HasValue && DateTime.UtcNow <= TrialEndsAt.Value;

    /// <summary>
    /// Billing email address (may differ from contact email).
    /// </summary>
    public string? BillingEmail { get; set; }

    /// <summary>
    /// External billing system customer ID (e.g., Stripe customer ID).
    /// </summary>
    public string? BillingCustomerId { get; set; }

    /// <summary>
    /// Indicates whether this organization is active.
    /// Inactive organizations cannot be accessed.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the organization was deactivated.
    /// </summary>
    public DateTime? DeactivatedDate { get; set; }

    /// <summary>
    /// Reason for deactivation (e.g., "Non-payment", "User request").
    /// </summary>
    public string? DeactivationReason { get; set; }

    /// <summary>
    /// Workspace settings stored as JSON.
    /// Example: {"allowPublicProjects": false, "requireMfa": true, "defaultProjectRole": "Viewer"}
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// Maximum number of members allowed (based on subscription).
    /// Null = unlimited.
    /// </summary>
    public int? MaxMembers { get; set; }

    /// <summary>
    /// Maximum number of projects allowed (based on subscription).
    /// Null = unlimited.
    /// </summary>
    public int? MaxProjects { get; set; }

    /// <summary>
    /// Maximum storage in GB (based on subscription).
    /// Null = unlimited.
    /// </summary>
    public int? MaxStorageGb { get; set; }

    /// <summary>
    /// Current storage used in bytes.
    /// </summary>
    public long StorageUsedBytes { get; set; }

    // Navigation properties

    /// <summary>
    /// Members of this organization.
    /// </summary>
    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();

    /// <summary>
    /// Pending invitations for this organization.
    /// </summary>
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    /// <summary>
    /// Projects belonging to this organization.
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
