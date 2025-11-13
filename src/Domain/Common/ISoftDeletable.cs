namespace IgnaCheck.Domain.Common;

/// <summary>
/// Marker interface for entities that support soft delete.
/// All entities inheriting from BaseAuditableEntity automatically support soft delete.
/// This interface can be used for additional soft delete-specific logic or constraints.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
