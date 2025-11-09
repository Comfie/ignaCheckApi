namespace IgnaCheck.Domain.Events;

/// <summary>
/// Domain event raised when a new project is created.
/// </summary>
public class ProjectCreatedEvent : BaseEvent
{
    public ProjectCreatedEvent(Project project)
    {
        Project = project;
    }

    public Project Project { get; }
}
