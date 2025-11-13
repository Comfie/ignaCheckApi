using System.Reflection;
using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Common;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IgnaCheck.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly IServiceProvider _serviceProvider;
    private ITenantService? _tenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    private ITenantService? TenantService
    {
        get
        {
            // Lazy resolution to avoid circular dependency
            // TenantService -> IApplicationDbContext -> ApplicationDbContext -> ITenantService
            _tenantService ??= _serviceProvider.GetService<ITenantService>();
            return _tenantService;
        }
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();

    public DbSet<Invitation> Invitations => Set<Invitation>();

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<ComplianceFramework> ComplianceFrameworks => Set<ComplianceFramework>();

    public DbSet<ComplianceControl> ComplianceControls => Set<ComplianceControl>();

    public DbSet<ProjectFramework> ProjectFrameworks => Set<ProjectFramework>();

    public DbSet<ComplianceFinding> ComplianceFindings => Set<ComplianceFinding>();

    public DbSet<FindingEvidence> FindingEvidence => Set<FindingEvidence>();

    public DbSet<RemediationTask> RemediationTasks => Set<RemediationTask>();

    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();

    public DbSet<FindingComment> FindingComments => Set<FindingComment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply global query filters for multi-tenancy and soft delete
        // This ensures queries automatically filter by:
        // 1. Current tenant's organization (multi-tenancy)
        // 2. Non-deleted entities (soft delete)
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var isSoftDeletable = typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType);
            var isTenantEntity = typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType);

            if (isSoftDeletable && isTenantEntity)
            {
                // Entity has both soft delete and multi-tenancy
                var method = SetGlobalQueryForTenantAndSoftDeleteMethod.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { builder });
            }
            else if (isSoftDeletable)
            {
                // Entity has only soft delete
                var method = SetGlobalQueryForSoftDeleteMethod.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { builder });
            }
            else if (isTenantEntity)
            {
                // Entity has only multi-tenancy
                var method = SetGlobalQueryMethod.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { builder });
            }
        }

        // Configure DateTime handling for PostgreSQL
        // PostgreSQL requires DateTime values to have Kind=UTC for timestamp with time zone
        ConfigureDateTimeProperties(builder);
    }

    private static void ConfigureDateTimeProperties(ModelBuilder builder)
    {
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue
                ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime())
                : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }

    private static readonly System.Reflection.MethodInfo SetGlobalQueryMethod =
        typeof(ApplicationDbContext).GetMethod(nameof(SetGlobalQuery),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    private static readonly System.Reflection.MethodInfo SetGlobalQueryForSoftDeleteMethod =
        typeof(ApplicationDbContext).GetMethod(nameof(SetGlobalQueryForSoftDelete),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    private static readonly System.Reflection.MethodInfo SetGlobalQueryForTenantAndSoftDeleteMethod =
        typeof(ApplicationDbContext).GetMethod(nameof(SetGlobalQueryForTenantAndSoftDelete),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    /// <summary>
    /// Sets global query filter for multi-tenancy only.
    /// </summary>
    private void SetGlobalQuery<T>(ModelBuilder builder) where T : class, ITenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e => TenantService == null || TenantService.GetCurrentTenantId() == null ||  e.OrganizationId == TenantService.GetCurrentTenantId());
    }

    /// <summary>
    /// Sets global query filter for soft delete only.
    /// Filters out entities where IsDeleted = true.
    /// </summary>
    private void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : BaseAuditableEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Sets global query filter for both multi-tenancy and soft delete.
    /// Combines tenant filtering and soft delete filtering.
    /// </summary>
    private void SetGlobalQueryForTenantAndSoftDelete<T>(ModelBuilder builder)
        where T : BaseAuditableEntity, ITenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e =>
            !e.IsDeleted &&
            (TenantService == null || TenantService.GetCurrentTenantId() == null || e.OrganizationId == TenantService.GetCurrentTenantId()));
    }
}
