using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Queries.GetProjectDetails;

/// <summary>
/// Query to get detailed information about a specific project.
/// </summary>
public record GetProjectDetailsQuery : IRequest<Result<ProjectDetailsDto>>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }
}

/// <summary>
/// Detailed project information DTO.
/// </summary>
public record ProjectDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public DateTime? TargetDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime? LastModified { get; init; }
    public string? LastModifiedBy { get; init; }
    public int DocumentCount { get; init; }
    public int FindingsCount { get; init; }
    public int FrameworkCount { get; init; }
    public int MemberCount { get; init; }
    public string UserRole { get; init; } = string.Empty;
    public List<ProjectMemberSummaryDto> Members { get; init; } = new();
}

/// <summary>
/// Project member summary.
/// </summary>
public record ProjectMemberSummaryDto
{
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedDate { get; init; }
}

/// <summary>
/// Handler for GetProjectDetailsQuery.
/// </summary>
public class GetProjectDetailsQueryHandler : IRequestHandler<GetProjectDetailsQuery, Result<ProjectDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;

    public GetProjectDetailsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<Result<ProjectDetailsDto>> Handle(GetProjectDetailsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<ProjectDetailsDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<ProjectDetailsDto>.Failure(new[] { "No workspace selected." });
        }

        // Get project with related data
        var project = await _context.Projects
            .Where(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value)
            .Select(p => new ProjectDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                TargetDate = p.TargetDate,
                CreatedAt = p.Created,
                CreatedBy = p.CreatedBy ?? "Unknown",
                LastModified = p.LastModified,
                LastModifiedBy = p.LastModifiedBy,
                DocumentCount = p.Documents.Count,
                FindingsCount = p.Findings.Count,
                FrameworkCount = p.ProjectFrameworks.Count,
                MemberCount = p.ProjectMembers.Count(pm => pm.IsActive),
                UserRole = p.ProjectMembers
                    .Where(pm => pm.UserId == _currentUser.Id && pm.IsActive)
                    .Select(pm => pm.Role.ToString())
                    .FirstOrDefault() ?? "None",
                Members = p.ProjectMembers
                    .Where(pm => pm.IsActive)
                    .OrderBy(pm => pm.Role)
                    .ThenBy(pm => pm.UserName)
                    .Select(pm => new ProjectMemberSummaryDto
                    {
                        UserId = pm.UserId,
                        UserName = pm.UserName,
                        UserEmail = pm.UserEmail,
                        Role = pm.Role.ToString(),
                        JoinedDate = pm.JoinedDate
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (project == null)
        {
            return Result<ProjectDetailsDto>.Failure(new[] { "Project not found." });
        }

        return Result<ProjectDetailsDto>.Success(project);
    }
}
