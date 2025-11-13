using IgnaCheck.Domain.Common;

namespace IgnaCheck.Domain.Events;

/// <summary>
/// Domain event raised when any entity is updated.
/// Used for automatic audit logging of significant property changes.
/// </summary>
public class EntityUpdatedEvent : BaseEvent
{
    public EntityUpdatedEvent(BaseAuditableEntity entity, string[] modifiedProperties)
    {
        Entity = entity;
        EntityType = entity.GetType().Name;
        EntityId = entity.Id;
        ModifiedProperties = modifiedProperties;
    }

    public BaseAuditableEntity Entity { get; }
    public string EntityType { get; }
    public Guid EntityId { get; }
    public string[] ModifiedProperties { get; }
}
