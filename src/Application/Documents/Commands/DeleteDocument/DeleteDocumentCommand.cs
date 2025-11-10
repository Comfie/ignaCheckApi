using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Infrastructure.Identity;

namespace IgnaCheck.Application.Documents.Commands.DeleteDocument;

/// <summary>
/// Command to delete a document from a project.
/// Deletes both the database record and the file from storage.
/// </summary>
public record DeleteDocumentCommand : IRequest<Result>
{
    /// <summary>
    /// Document ID to delete.
    /// </summary>
    public Guid DocumentId { get; init; }
}

/// <summary>
/// Handler for DeleteDocumentCommand.
/// </summary>
public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly IFileStorageService _fileStorageService;

    public DeleteDocumentCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService,
        IIdentityService identityService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _identityService = identityService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result.Failure(new[] { "No workspace selected." });
        }

        // Get document with project and organization
        var document = await _context.Documents
            .Include(d => d.Project)
                .ThenInclude(p => p.ProjectMembers)
            .Include(d => d.Project)
                .ThenInclude(p => p.Organization)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.OrganizationId == organizationId.Value, cancellationToken);

        if (document == null)
        {
            return Result.Failure(new[] { "Document not found." });
        }

        // Check if user has permission (Owner or Contributor)
        var userMember = document.Project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null || userMember.Role == ProjectRole.Viewer)
        {
            return Result.Failure(new[] { "You do not have permission to delete documents from this project." });
        }

        // Get user details for activity log
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        var appUser = user as IgnaCheck.Infrastructure.Identity.ApplicationUser;
        var userName = appUser != null
            ? $"{appUser.FirstName} {appUser.LastName}".Trim()
            : "Unknown User";

        // Delete file from storage
        try
        {
            await _fileStorageService.DeleteFileAsync(document.StoragePath, cancellationToken);
        }
        catch (Exception)
        {
            // Log but continue - file may already be deleted or storage error
        }

        // Update organization storage usage
        document.Project.Organization.StorageUsedBytes -= document.FileSizeBytes;
        if (document.Project.Organization.StorageUsedBytes < 0)
        {
            document.Project.Organization.StorageUsedBytes = 0;
        }

        // Log activity before deletion
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = document.ProjectId,
            UserId = _currentUser.Id,
            UserName = userName,
            UserEmail = appUser?.Email ?? "unknown@example.com",
            ActivityType = ActivityType.DocumentDeleted,
            EntityType = "Document",
            EntityId = document.Id,
            EntityName = document.FileName,
            Description = $"Deleted document '{document.FileName}' from project '{document.Project.Name}'",
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                FileName = document.FileName,
                FileSizeBytes = document.FileSizeBytes,
                Category = document.Category.ToString()
            }),
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        // Delete document from database (cascade will handle related records)
        _context.Documents.Remove(document);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
