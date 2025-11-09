namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for managing multi-tenant context and operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current tenant/organization ID from the current user context.
    /// </summary>
    Guid? GetCurrentTenantId();

    /// <summary>
    /// Gets the current tenant/organization.
    /// </summary>
    Task<Organization?> GetCurrentTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the tenant ID for the current request context.
    /// Used primarily in testing scenarios.
    /// </summary>
    void SetCurrentTenantId(Guid tenantId);
}
