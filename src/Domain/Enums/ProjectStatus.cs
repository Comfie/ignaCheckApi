namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a compliance audit project.
/// </summary>
public enum ProjectStatus
{
    Draft = 0,
    Active = 1,
    InProgress = 2,
    Completed = 3,
    Archived = 4,
    OnHold = 5
}
