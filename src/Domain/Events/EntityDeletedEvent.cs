using IgnaCheck.Domain.Common;

namespace IgnaCheck.Domain.Events;

/// <summary>
/// Domain event raised when any entity is soft deleted.
/// Used for automatic audit logging of deletions.
/// </summary>
public class EntityDeletedEvent : BaseEvent
{
    public EntityDeletedEvent(BaseAuditableEntity entity)
    {
        Entity = entity;
        EntityType = entity.GetType().Name;
        EntityId = entity.Id;
    }

    public BaseAuditableEntity Entity { get; }
    public string EntityType { get; }
    public Guid EntityId { get; }
}
