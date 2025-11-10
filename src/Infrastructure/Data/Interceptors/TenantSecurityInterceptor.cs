using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace IgnaCheck.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor to enforce multi-tenancy by automatically setting OrganizationId
/// on all entities that implement ITenantEntity.
/// </summary>
public class TenantSecurityInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private ITenantService? _tenantService;

    public TenantSecurityInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private ITenantService? TenantService
    {
        get
        {
            // Lazy resolution to avoid circular dependency
            // ApplicationDbContext -> TenantSecurityInterceptor -> ITenantService -> IApplicationDbContext
            _tenantService ??= _serviceProvider.GetService<ITenantService>();
            return _tenantService;
        }
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyTenantFilter(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyTenantFilter(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantFilter(DbContext? context)
    {
        if (context == null) return;

        var currentTenantId = TenantService?.GetCurrentTenantId();

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Automatically set the organization ID on new entities
                    if (currentTenantId.HasValue)
                    {
                        entry.Entity.OrganizationId = currentTenantId.Value;
                    }
                    break;

                case EntityState.Modified:
                    // Prevent changing the organization ID
                    var originalOrgId = entry.OriginalValues.GetValue<Guid>(nameof(ITenantEntity.OrganizationId));
                    var currentOrgId = entry.Entity.OrganizationId;

                    if (originalOrgId != currentOrgId)
                    {
                        throw new InvalidOperationException(
                            "Cannot change the OrganizationId of an entity. This violates tenant isolation.");
                    }

                    // Verify the entity belongs to the current tenant
                    if (currentTenantId.HasValue && entry.Entity.OrganizationId != currentTenantId.Value)
                    {
                        throw new InvalidOperationException(
                            $"Cannot modify entity from another organization. Current tenant: {currentTenantId}, Entity tenant: {entry.Entity.OrganizationId}");
                    }
                    break;

                case EntityState.Deleted:
                    // Verify the entity belongs to the current tenant
                    if (currentTenantId.HasValue && entry.Entity.OrganizationId != currentTenantId.Value)
                    {
                        throw new InvalidOperationException(
                            $"Cannot delete entity from another organization. Current tenant: {currentTenantId}, Entity tenant: {entry.Entity.OrganizationId}");
                    }
                    break;
            }
        }
    }
}
