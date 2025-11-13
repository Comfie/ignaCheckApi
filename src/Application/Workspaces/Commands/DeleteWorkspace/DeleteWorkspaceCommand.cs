using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Constants;
using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Workspaces.Commands.DeleteWorkspace;

/// <summary>
/// Command to permanently delete a workspace and all associated data.
/// This is a destructive operation that cannot be undone.
/// </summary>
public record DeleteWorkspaceCommand : IRequest<Result>
{
    /// <summary>
    /// Workspace/Organization ID to delete.
    /// </summary>
    public Guid OrganizationId { get; init; }

    /// <summary>
    /// Confirmation text - must match workspace name exactly.
    /// </summary>
    public string ConfirmationText { get; init; } = string.Empty;

    /// <summary>
    /// Additional confirmation flag.
    /// </summary>
    public bool ConfirmDeletion { get; init; }
}

/// <summary>
/// Validator for DeleteWorkspaceCommand.
/// </summary>
public class DeleteWorkspaceCommandValidator : AbstractValidator<DeleteWorkspaceCommand>
{
    public DeleteWorkspaceCommandValidator()
    {
        RuleFor(v => v.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required.");

        RuleFor(v => v.ConfirmationText)
            .NotEmpty().WithMessage("Confirmation text is required for workspace deletion.");

        RuleFor(v => v.ConfirmDeletion)
            .Equal(true).WithMessage("You must confirm deletion by setting ConfirmDeletion to true.");
    }
}

/// <summary>
/// Handler for DeleteWorkspaceCommand.
/// </summary>
public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IFileStorageService _fileStorageService;

    public DeleteWorkspaceCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Get organization with members
        var organization = await _context.Organizations
            .Include(o => o.Members)
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            return Result.Failure(new[] { "Workspace not found." });
        }

        // Verify user is an owner
        var member = organization.Members.FirstOrDefault(m => m.UserId == _currentUser.Id);
        if (member == null || member.Role != WorkspaceRoles.Owner)
        {
            return Result.Failure(new[] { "Access denied. Only workspace owners can delete the workspace." });
        }

        // Verify confirmation text matches organization name (case-insensitive)
        if (!string.Equals(request.ConfirmationText.Trim(), organization.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(new[] { $"Confirmation text must match the workspace name exactly: '{organization.Name}'" });
        }

        // Get all projects for cleanup
        var projects = await _context.Projects
            .Where(p => p.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);

        // Get all documents for file cleanup
        var documents = await _context.Documents
            .Include(d => d.Project)
            .Where(d => d.Project.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);

        // Delete document files from storage
        foreach (var document in documents)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(document.StoragePath))
                {
                    await _fileStorageService.DeleteFileAsync(document.StoragePath, cancellationToken);
                }
            }
            catch (Exception)
            {
                // Continue even if file deletion fails
                // This handles cases where files were already deleted or storage is unavailable
            }
        }

        // Delete workspace (soft delete via interceptor)
        // The SoftDeleteInterceptor will:
        // 1. Automatically soft delete all entities (set IsDeleted = true)
        // 2. Raise EntityDeletedEvent for each entity
        // 3. EntityDeletedEventHandler will create audit logs automatically
        // 4. Cascade deletes will soft delete all related entities

        // Delete all related data - EF Core cascade will handle soft deleting children
        // Note: We still need to explicitly remove non-auditable entities (like ActivityLogs)

        // Delete activity logs (these are not BaseAuditableEntity, so hard delete)
        var activityLogs = await _context.ActivityLogs
            .Where(a => a.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.ActivityLogs.RemoveRange(activityLogs);

        // Soft delete the organization - cascade will handle children
        _context.Organizations.Remove(organization);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
