namespace IgnaCheck.Domain.Events;

/// <summary>
/// Domain event raised when a project's status changes.
/// </summary>
public class ProjectStatusChangedEvent : BaseEvent
{
    public ProjectStatusChangedEvent(Project project, ProjectStatus oldStatus, ProjectStatus newStatus)
    {
        Project = project;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public Project Project { get; }
    public ProjectStatus OldStatus { get; }
    public ProjectStatus NewStatus { get; }
}
