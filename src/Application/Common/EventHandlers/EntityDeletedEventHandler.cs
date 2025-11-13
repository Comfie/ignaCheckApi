using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Application.Common.EventHandlers;

/// <summary>
/// Handles EntityDeletedEvent by creating an audit log entry automatically.
/// This ensures all entity deletions are logged consistently.
/// </summary>
public class EntityDeletedEventHandler : INotificationHandler<EntityDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<EntityDeletedEventHandler> _logger;

    public EntityDeletedEventHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService,
        IIdentityService identityService,
        ILogger<EntityDeletedEventHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task Handle(EntityDeletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _tenantService.GetCurrentTenantId();
            if (organizationId == null)
            {
                _logger.LogWarning("Cannot log deletion: No organization context available for {EntityType} {EntityId}",
                    notification.EntityType, notification.EntityId);
                return;
            }

            // Get user details
            var userId = _currentUser.Id ?? "system";
            var user = await _identityService.GetUserByIdAsync(userId);
            var userName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "System";
            var userEmail = user?.Email ?? "system@ignacheck.ai";

            // Determine activity type based on entity type
            var activityType = GetActivityType(notification.EntityType);

            // Get entity name if available
            var entityName = GetEntityName(notification.Entity);

            // Get project ID if applicable
            var projectId = GetProjectId(notification.Entity);

            // Build metadata
            var metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                EntityType = notification.EntityType,
                DeletedAt = notification.Entity.DeletedAt,
                DeletedBy = notification.Entity.DeletedBy,
                IsDeleted = notification.Entity.IsDeleted
            });

            // Create activity log
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
                Description = BuildDescription(notification.EntityType, entityName, "deleted"),
                Metadata = metadata,
                OccurredAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);

            _logger.LogInformation("Logged deletion of {EntityType} {EntityId} by user {UserId}",
                notification.EntityType, notification.EntityId, userId);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - we don't want audit logging failures to break the application
            _logger.LogError(ex, "Error logging entity deletion for {EntityType} {EntityId}",
                notification.EntityType, notification.EntityId);
        }
    }

    private static ActivityType GetActivityType(string entityType)
    {
        return entityType switch
        {
            nameof(Project) => ActivityType.ProjectDeleted,
            nameof(Document) => ActivityType.DocumentDeleted,
            nameof(ComplianceFinding) => ActivityType.FindingDeleted,
            nameof(Organization) => ActivityType.WorkspaceDeleted,
            nameof(OrganizationMember) => ActivityType.UserRemoved,
            nameof(ProjectMember) => ActivityType.ProjectMemberRemoved,
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

    private static string BuildDescription(string entityType, string? entityName, string action)
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

        return entityName != null
            ? $"{char.ToUpper(action[0])}{action.Substring(1)} {friendlyType} '{entityName}'"
            : $"{char.ToUpper(action[0])}{action.Substring(1)} {friendlyType}";
    }
}
