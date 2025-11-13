using IgnaCheck.Domain.Common;

namespace IgnaCheck.Domain.Events;

/// <summary>
/// Domain event raised when any entity is created.
/// Used for automatic audit logging of entity creation.
/// </summary>
public class EntityCreatedEvent : BaseEvent
{
    public EntityCreatedEvent(BaseAuditableEntity entity)
    {
        Entity = entity;
        EntityType = entity.GetType().Name;
        EntityId = entity.Id;
    }

    public BaseAuditableEntity Entity { get; }
    public string EntityType { get; }
    public Guid EntityId { get; }
}
