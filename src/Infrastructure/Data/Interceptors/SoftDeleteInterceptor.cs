using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Common;
using IgnaCheck.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor that converts hard deletes to soft deletes for entities inheriting from BaseAuditableEntity.
/// When an entity is deleted, it sets IsDeleted = true instead of actually removing it from the database.
/// Also raises EntityDeletedEvent for audit logging.
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _dateTime;
    private readonly ILogger<SoftDeleteInterceptor> _logger;

    public SoftDeleteInterceptor(
        IUser user,
        TimeProvider dateTime,
        ILogger<SoftDeleteInterceptor> logger)
    {
        _user = user;
        _dateTime = dateTime;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        HandleSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        HandleSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void HandleSoftDelete(DbContext? context)
    {
        if (context == null) return;

        var utcNow = _dateTime.GetUtcNow().UtcDateTime;
        var deletedEntitiesCount = 0;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                var entityType = entry.Entity.GetType().Name;
                var entityId = entry.Entity.Id;

                _logger.LogDebug(
                    "Converting hard delete to soft delete for {EntityType} with ID {EntityId}",
                    entityType,
                    entityId);

                // Convert hard delete to soft delete
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = utcNow;
                entry.Entity.DeletedBy = _user.Id;

                // Add domain event for deletion (will be handled by DispatchDomainEventsInterceptor)
                entry.Entity.AddDomainEvent(new EntityDeletedEvent(entry.Entity));

                deletedEntitiesCount++;

                _logger.LogInformation(
                    "Soft deleted {EntityType} with ID {EntityId} by user {UserId} at {DeletedAt}",
                    entityType,
                    entityId,
                    _user.Id,
                    utcNow);
            }
        }

        if (deletedEntitiesCount > 0)
        {
            _logger.LogInformation(
                "Soft delete interceptor processed {Count} entities",
                deletedEntitiesCount);
        }
    }
}
