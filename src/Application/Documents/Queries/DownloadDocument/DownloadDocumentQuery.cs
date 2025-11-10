using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Infrastructure.Identity;

namespace IgnaCheck.Application.Documents.Queries.DownloadDocument;

/// <summary>
/// Query to download a document file.
/// </summary>
public record DownloadDocumentQuery : IRequest<Result<DownloadDocumentResponse>>
{
    /// <summary>
    /// Document ID to download.
    /// </summary>
    public Guid DocumentId { get; init; }
}

/// <summary>
/// Response containing the file stream and metadata.
/// </summary>
public record DownloadDocumentResponse
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
}

/// <summary>
/// Handler for DownloadDocumentQuery.
/// </summary>
public class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, Result<DownloadDocumentResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly IFileStorageService _fileStorageService;

    public DownloadDocumentQueryHandler(
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

    public async Task<Result<DownloadDocumentResponse>> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<DownloadDocumentResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<DownloadDocumentResponse>.Failure(new[] { "No workspace selected." });
        }

        // Get document with project members
        var document = await _context.Documents
            .Include(d => d.Project)
                .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.OrganizationId == organizationId.Value, cancellationToken);

        if (document == null)
        {
            return Result<DownloadDocumentResponse>.Failure(new[] { "Document not found." });
        }

        // Check if user has access to project
        var userMember = document.Project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null)
        {
            return Result<DownloadDocumentResponse>.Failure(new[] { "You do not have access to this document." });
        }

        // Download file from storage
        var (fileStream, contentType) = await _fileStorageService.DownloadFileAsync(document.StoragePath, cancellationToken);

        // Get user details for activity log
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        var appUser = user as IgnaCheck.Infrastructure.Identity.ApplicationUser;
        var userName = appUser != null
            ? $"{appUser.FirstName} {appUser.LastName}".Trim()
            : "Unknown User";

        // Log download activity (fire and forget - don't block response)
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = document.ProjectId,
            UserId = _currentUser.Id,
            UserName = userName,
            UserEmail = appUser?.Email ?? "unknown@example.com",
            ActivityType = ActivityType.DocumentDownloaded,
            EntityType = "Document",
            EntityId = document.Id,
            EntityName = document.FileName,
            Description = $"Downloaded document '{document.FileName}'",
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new DownloadDocumentResponse
        {
            FileStream = fileStream,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes
        };

        return Result<DownloadDocumentResponse>.Success(response);
    }
}
