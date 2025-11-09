namespace IgnaCheck.Domain.Events;

/// <summary>
/// Domain event raised when a new organization is created.
/// </summary>
public class OrganizationCreatedEvent : BaseEvent
{
    public OrganizationCreatedEvent(Organization organization)
    {
        Organization = organization;
    }

    public Organization Organization { get; }
}
