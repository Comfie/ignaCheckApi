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

        // Apply global query filters for multi-tenancy
        // This ensures queries automatically filter by the current tenant's organization
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
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

    private void SetGlobalQuery<T>(ModelBuilder builder) where T : class, ITenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e => TenantService == null || TenantService.GetCurrentTenantId() == null ||  e.OrganizationId == TenantService.GetCurrentTenantId());
    }
}
