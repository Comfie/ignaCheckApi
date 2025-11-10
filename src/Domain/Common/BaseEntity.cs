using System.ComponentModel.DataAnnotations.Schema;

namespace IgnaCheck.Domain.Common;

public abstract class BaseEntity
{
    // Using Guid as the primary key type for all entities
    // This aligns with the foreign key usage throughout the codebase
    public Guid Id { get; set; }

    private readonly List<BaseEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
