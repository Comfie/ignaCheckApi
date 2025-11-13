using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Application.Common.EventHandlers;

/// <summary>
/// Handles EntityUpdatedEvent by creating audit log entries for significant updates.
/// Only logs updates to important properties (not every field change).
/// </summary>
public class EntityUpdatedEventHandler : INotificationHandler<EntityUpdatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<EntityUpdatedEventHandler> _logger;

    // Properties to track for audit logging (ignore noise like timestamps)
    private static readonly HashSet<string> _auditedProperties = new()
    {
        // Project properties
        nameof(Project.Name),
        nameof(Project.Description),
        nameof(Project.Status),
        nameof(Project.TargetDate),

        // Finding properties
        nameof(ComplianceFinding.WorkflowStatus),
        nameof(ComplianceFinding.AssignedTo),
        nameof(ComplianceFinding.Title),
        nameof(ComplianceFinding.Description),

        // Document properties
        nameof(Document.FileName),
        nameof(Document.Category),

        // Organization properties
        nameof(Organization.Name),
        nameof(Organization.SubscriptionTier),
        nameof(Organization.IsActive),

        // Member role changes
        "Role"
    };

    public EntityUpdatedEventHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService,
        IIdentityService identityService,
        ILogger<EntityUpdatedEventHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task Handle(EntityUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Only log updates to significant properties
            var significantChanges = notification.ModifiedProperties
                .Where(p => _auditedProperties.Contains(p))
                .ToArray();

            if (!significantChanges.Any())
            {
                // No significant changes to log
                return;
            }

            var organizationId = _tenantService.GetCurrentTenantId();
            if (organizationId == null)
            {
                _logger.LogWarning("Cannot log update: No organization context available for {EntityType} {EntityId}",
                    notification.EntityType, notification.EntityId);
                return;
            }

            var userId = _currentUser.Id ?? "system";
            var user = await _identityService.GetUserByIdAsync(userId);
            var userName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "System";
            var userEmail = user?.Email ?? "system@ignacheck.ai";

            var activityType = GetActivityType(notification.EntityType);
            var entityName = GetEntityName(notification.Entity);
            var projectId = GetProjectId(notification.Entity);

            var metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                ModifiedProperties = significantChanges,
                EntityType = notification.EntityType
            });

            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId.Value,
                ProjectId = projectId,
                UserId = userId,
                UserName = userName,
                UserEmail = userEmail,
                ActivityType = activityType,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                EntityName = entityName,
                Description = BuildDescription(notification.EntityType, entityName, "updated", significantChanges),
                Metadata = metadata,
                OccurredAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);

            _logger.LogInformation("Logged update of {EntityType} {EntityId} by user {UserId}. Changed: {Properties}",
                notification.EntityType, notification.EntityId, userId, string.Join(", ", significantChanges));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging entity update for {EntityType} {EntityId}",
                notification.EntityType, notification.EntityId);
        }
    }

    private static ActivityType GetActivityType(string entityType)
    {
        return entityType switch
        {
            nameof(Project) => ActivityType.ProjectUpdated,
            nameof(Document) => ActivityType.DocumentUpdated,
            nameof(ComplianceFinding) => ActivityType.FindingUpdated,
            nameof(Organization) => ActivityType.WorkspaceUpdated,
            nameof(OrganizationMember) => ActivityType.UserRoleChanged,
            nameof(ProjectMember) => ActivityType.ProjectMemberRoleChanged,
            _ => ActivityType.Other
        };
    }

    private static string? GetEntityName(Domain.Common.BaseAuditableEntity entity)
    {
        return entity switch
        {
            Project p => p.Name,
            Document d => d.FileName,
            Organization o => o.Name,
            ComplianceFinding f => f.Title,
            _ => null
        };
    }

    private static Guid? GetProjectId(Domain.Common.BaseAuditableEntity entity)
    {
        return entity switch
        {
            Document d => d.ProjectId,
            ComplianceFinding f => f.ProjectId,
            RemediationTask t => t.ProjectId,
            ProjectMember pm => pm.ProjectId,
            _ => null
        };
    }

    private static string BuildDescription(string entityType, string? entityName, string action, string[] modifiedProperties)
    {
        var friendlyType = entityType switch
        {
            nameof(Project) => "project",
            nameof(Document) => "document",
            nameof(ComplianceFinding) => "finding",
            nameof(Organization) => "workspace",
            nameof(OrganizationMember) => "member",
            nameof(ProjectMember) => "project member",
            _ => entityType.ToLower()
        };

        var baseDescription = entityName != null
            ? $"{char.ToUpper(action[0])}{action.Substring(1)} {friendlyType} '{entityName}'"
            : $"{char.ToUpper(action[0])}{action.Substring(1)} {friendlyType}";

        // Add details about what changed if there are only a few properties
        if (modifiedProperties.Length <= 3)
        {
            var propertyList = string.Join(", ", modifiedProperties);
            return $"{baseDescription} ({propertyList})";
        }

        return baseDescription;
    }
}
