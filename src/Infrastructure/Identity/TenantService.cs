using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IgnaCheck.Infrastructure.Identity;

/// <summary>
/// Service for managing multi-tenant context.
/// Extracts the organization ID from the authenticated user's JWT claims.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUser _currentUser;
    private readonly IApplicationDbContext _context;
    private Guid? _overrideTenantId; // For testing scenarios

    public TenantService(IUser currentUser, IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public Guid? GetCurrentTenantId()
    {
        // If there's an override (testing), use it
        if (_overrideTenantId.HasValue)
        {
            return _overrideTenantId;
        }

        // Extract organization ID from user claims (set during workspace switching)
        return _currentUser.OrganizationId;
    }

    public async Task<Organization?> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetCurrentTenantId();

        if (!tenantId.HasValue)
        {
            return null;
        }

        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == tenantId.Value && o.IsActive, cancellationToken);
    }

    public void SetCurrentTenantId(Guid tenantId)
    {
        _overrideTenantId = tenantId;
    }
}
