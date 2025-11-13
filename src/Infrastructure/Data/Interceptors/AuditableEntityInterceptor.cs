using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Common;
using IgnaCheck.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IgnaCheck.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _dateTime;

    public AuditableEntityInterceptor(
        IUser user,
        TimeProvider dateTime)
    {
        _user = user;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = _dateTime.GetUtcNow().UtcDateTime;

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.Created = utcNow;

                    // Raise domain event for entity creation
                    entry.Entity.AddDomainEvent(new EntityCreatedEvent(entry.Entity));
                }

                if (entry.State == EntityState.Modified)
                {
                    // Get modified properties for audit logging
                    var modifiedProperties = entry.Properties
                        .Where(p => p.IsModified &&
                                    p.Metadata.Name != nameof(BaseAuditableEntity.LastModified) &&
                                    p.Metadata.Name != nameof(BaseAuditableEntity.LastModifiedBy))
                        .Select(p => p.Metadata.Name)
                        .ToArray();

                    // Only raise event if there are significant changes (not just audit fields)
                    if (modifiedProperties.Any())
                    {
                        entry.Entity.AddDomainEvent(new EntityUpdatedEvent(entry.Entity, modifiedProperties));
                    }
                }

                entry.Entity.LastModifiedBy = _user.Id;
                entry.Entity.LastModified = utcNow;
            }
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r => 
            r.TargetEntry != null && 
            r.TargetEntry.Metadata.IsOwned() && 
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
