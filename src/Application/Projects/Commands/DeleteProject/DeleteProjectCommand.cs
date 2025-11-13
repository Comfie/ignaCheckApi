using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Commands.DeleteProject;

/// <summary>
/// Command to permanently delete a project and all its associated data.
/// Only project owners can delete projects.
/// </summary>
public record DeleteProjectCommand : IRequest<Result>
{
    /// <summary>
    /// Project ID to delete.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Confirmation string (must match project name).
    /// </summary>
    public string ConfirmationName { get; init; } = string.Empty;
}

/// <summary>
/// Handler for DeleteProjectCommand.
/// </summary>
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly IFileStorageService _fileStorageService;

    public DeleteProjectCommandHandler(
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

    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
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

        // Get project with all related data
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result.Failure(new[] { "Project not found." });
        }

        // Check if user is project owner
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null || userMember.Role != ProjectRole.Owner)
        {
            return Result.Failure(new[] { "Only project owners can delete projects." });
        }

        // Verify confirmation name
        if (request.ConfirmationName != project.Name)
        {
            return Result.Failure(new[] { "Confirmation name does not match project name." });
        }

        // Delete all documents from storage
        foreach (var document in project.Documents)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(document.StoragePath, cancellationToken);
            }
            catch (Exception)
            {
                // Log but continue - file may already be deleted
            }
        }

        // Delete project (soft delete via interceptor)
        // The SoftDeleteInterceptor will:
        // 1. Set IsDeleted = true instead of removing from database
        // 2. Raise EntityDeletedEvent
        // 3. EntityDeletedEventHandler will create the audit log automatically
        _context.Projects.Remove(project);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
