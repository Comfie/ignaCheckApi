using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IgnaCheck.Infrastructure.Identity;

/// <summary>
/// Service for managing multi-tenant context.
/// In production, this would extract the organization ID from the authenticated user's claims.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUser _currentUser;
    private readonly IApplicationDbContext _context;
    private int? _overrideTenantId; // For testing scenarios

    public TenantService(IUser currentUser, IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public int? GetCurrentTenantId()
    {
        // If there's an override (testing), use it
        if (_overrideTenantId.HasValue)
        {
            return _overrideTenantId;
        }

        // In production, extract from user claims
        // For now, return null as a placeholder
        // TODO: Implement claim-based tenant resolution
        // e.g., return _currentUser.OrganizationId;

        return null;
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

    public void SetCurrentTenantId(int tenantId)
    {
        _overrideTenantId = tenantId;
    }
}
