using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Application.Common.EventHandlers;

/// <summary>
/// Handles EntityCreatedEvent by creating audit log entries for entity creation.
/// Only logs creation of significant entities (not every record).
/// </summary>
public class EntityCreatedEventHandler : INotificationHandler<EntityCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<EntityCreatedEventHandler> _logger;

    // Entity types to log creation for (ignore noise like ActivityLog itself)
    private static readonly HashSet<string> _loggedEntityTypes = new()
    {
        nameof(Project),
        nameof(Document),
        nameof(ComplianceFinding),
        nameof(Organization),
        nameof(OrganizationMember),
        nameof(ProjectMember),
        nameof(RemediationTask)
    };

    public EntityCreatedEventHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService,
        IIdentityService identityService,
        ILogger<EntityCreatedEventHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task Handle(EntityCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Only log creation of significant entities
            if (!_loggedEntityTypes.Contains(notification.EntityType))
            {
                return;
            }

            var organizationId = _tenantService.GetCurrentTenantId();
            if (organizationId == null)
            {
                _logger.LogWarning("Cannot log creation: No organization context available for {EntityType} {EntityId}",
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
                EntityType = notification.EntityType,
                CreatedAt = notification.Entity.Created,
                CreatedBy = notification.Entity.CreatedBy
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
                Description = BuildDescription(notification.EntityType, entityName, "created"),
                Metadata = metadata,
                OccurredAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);

            _logger.LogInformation("Logged creation of {EntityType} {EntityId} by user {UserId}",
                notification.EntityType, notification.EntityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging entity creation for {EntityType} {EntityId}",
                notification.EntityType, notification.EntityId);
        }
    }

    private static ActivityType GetActivityType(string entityType)
    {
        return entityType switch
        {
            nameof(Project) => ActivityType.ProjectCreated,
            nameof(Document) => ActivityType.DocumentUploaded,
            nameof(ComplianceFinding) => ActivityType.FindingCreated,
            nameof(Organization) => ActivityType.WorkspaceCreated,
            nameof(OrganizationMember) => ActivityType.UserJoined,
            nameof(ProjectMember) => ActivityType.ProjectMemberAdded,
            nameof(RemediationTask) => ActivityType.TaskCreated,
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
            RemediationTask t => t.Title,
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
            nameof(RemediationTask) => "task",
            _ => entityType.ToLower()
        };

        return entityName != null
            ? $"{char.ToUpper(action[0])}{action.Substring(1)} {friendlyType} '{entityName}'"
            : $"{char.ToUpper(action[0])}{action.Substring(1)} {friendlyType}";
    }
}
