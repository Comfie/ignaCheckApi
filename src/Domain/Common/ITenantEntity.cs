namespace IgnaCheck.Domain.Common;

/// <summary>
/// Marker interface for entities that belong to a specific tenant/organization.
/// Entities implementing this interface will have tenant isolation applied automatically.
/// </summary>
public interface ITenantEntity
{
    Guid OrganizationId { get; set; }
}
