using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Documents.Queries.GetDocumentsList;

/// <summary>
/// Query to get all documents in a project.
/// </summary>
public record GetDocumentsListQuery : IRequest<Result<List<DocumentDto>>>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Filter by document category (optional).
    /// </summary>
    public DocumentCategory? Category { get; init; }

    /// <summary>
    /// Search by filename or description (optional).
    /// </summary>
    public string? SearchTerm { get; init; }
}

/// <summary>
/// Document DTO for list display.
/// </summary>
public record DocumentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsTextExtracted { get; init; }
    public int Version { get; init; }
    public bool IsLatestVersion { get; init; }
    public DateTime UploadedDate { get; init; }
    public string? UploadedBy { get; init; }
    public int? PageCount { get; init; }
}

/// <summary>
/// Handler for GetDocumentsListQuery.
/// </summary>
public class GetDocumentsListQueryHandler : IRequestHandler<GetDocumentsListQuery, Result<List<DocumentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;

    public GetDocumentsListQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<Result<List<DocumentDto>>> Handle(GetDocumentsListQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<DocumentDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<List<DocumentDto>>.Failure(new[] { "No workspace selected." });
        }

        // Verify project exists and user has access
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result<List<DocumentDto>>.Failure(new[] { "Project not found." });
        }

        // Check if user has access to project
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null)
        {
            return Result<List<DocumentDto>>.Failure(new[] { "You do not have access to this project." });
        }

        // Build query
        var query = _context.Documents
            .Where(d => d.ProjectId == request.ProjectId);

        // Apply filters
        if (request.Category.HasValue)
        {
            query = query.Where(d => d.Category == request.Category.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.FileName.ToLower().Contains(searchTerm) ||
                d.DisplayName.ToLower().Contains(searchTerm) ||
                (d.Description != null && d.Description.ToLower().Contains(searchTerm))
            );
        }

        // Execute query
        var documents = await query
            .OrderByDescending(d => d.UploadedDate)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                DisplayName = d.DisplayName,
                Description = d.Description,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                Category = d.Category.ToString(),
                IsTextExtracted = d.IsTextExtracted,
                Version = d.Version,
                IsLatestVersion = d.IsLatestVersion,
                UploadedDate = d.UploadedDate,
                UploadedBy = d.UploadedBy,
                PageCount = d.PageCount
            })
            .ToListAsync(cancellationToken);

        return Result<List<DocumentDto>>.Success(documents);
    }
}
