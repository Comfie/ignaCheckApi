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
                // Continue even if file deletion fails - we'll still delete the database records
                // This handles cases where files were already deleted or storage is unavailable
            }
        }

        // Delete all related data (cascade deletes will handle most of this, but we'll be explicit)

        // Delete activity logs
        var activityLogs = await _context.ActivityLogs
            .Where(a => a.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.ActivityLogs.RemoveRange(activityLogs);

        // Delete notifications
        var memberUserIds = organization.Members.Select(m => m.UserId).ToList();
        var notifications = await _context.Notifications
            .Where(n => memberUserIds.Contains(n.UserId))
            .ToListAsync(cancellationToken);
        _context.Notifications.RemoveRange(notifications);

        // Delete invitations
        var invitations = await _context.Invitations
            .Where(i => i.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.Invitations.RemoveRange(invitations);

        // Delete finding comments
        var findingComments = await _context.FindingComments
            .Include(fc => fc.Finding)
            .Where(fc => fc.Finding.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.FindingComments.RemoveRange(findingComments);

        // Delete task comments
        var taskComments = await _context.TaskComments
            .Include(tc => tc.Task)
            .Where(tc => tc.Task.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.TaskComments.RemoveRange(taskComments);

        // Delete finding evidence
        var findingEvidence = await _context.FindingEvidence
            .Include(fe => fe.Finding)
            .Where(fe => fe.Finding.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.FindingEvidence.RemoveRange(findingEvidence);

        // Delete findings
        var findings = await _context.ComplianceFindings
            .Where(f => f.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.ComplianceFindings.RemoveRange(findings);

        // Delete task attachments
        var taskAttachments = await _context.Set<TaskAttachment>()
            .Include(ta => ta.Task)
            .Where(ta => ta.Task.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.Set<TaskAttachment>().RemoveRange(taskAttachments);

        // Delete tasks
        var tasks = await _context.RemediationTasks
            .Where(t => t.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.RemediationTasks.RemoveRange(tasks);

        // Delete documents
        _context.Documents.RemoveRange(documents);

        // Delete project frameworks
        var projectFrameworks = await _context.ProjectFrameworks
            .Include(pf => pf.Project)
            .Where(pf => pf.Project.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.ProjectFrameworks.RemoveRange(projectFrameworks);

        // Delete project members
        var projectMembers = await _context.ProjectMembers
            .Include(pm => pm.Project)
            .Where(pm => pm.Project.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);
        _context.ProjectMembers.RemoveRange(projectMembers);

        // Delete projects
        _context.Projects.RemoveRange(projects);

        // Delete organization members
        _context.OrganizationMembers.RemoveRange(organization.Members);

        // Finally, delete the organization itself
        _context.Organizations.Remove(organization);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
