using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace IgnaCheck.Application.Documents.Commands.UploadDocument;

/// <summary>
/// Command to upload a single document to a project.
/// </summary>
public record UploadDocumentCommand : IRequest<Result<UploadDocumentResponse>>
{
    /// <summary>
    /// Project ID to upload document to.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// The file to upload.
    /// </summary>
    public IFormFile File { get; init; } = null!;

    /// <summary>
    /// Document category (optional).
    /// </summary>
    public DocumentCategory? Category { get; init; }

    /// <summary>
    /// Document description or notes (optional).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Tags for categorization (optional, comma-separated).
    /// </summary>
    public string? Tags { get; init; }
}

/// <summary>
/// Response for successful document upload.
/// </summary>
public record UploadDocumentResponse
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DocumentCategory Category { get; init; }
    public bool IsTextExtracted { get; init; }
    public DateTime UploadedDate { get; init; }
}

/// <summary>
/// Handler for UploadDocumentCommand.
/// </summary>
public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<UploadDocumentResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentParsingService _documentParsingService;

    public UploadDocumentCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService,
        IIdentityService identityService,
        IFileStorageService fileStorageService,
        IDocumentParsingService documentParsingService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _identityService = identityService;
        _fileStorageService = fileStorageService;
        _documentParsingService = documentParsingService;
    }

    public async Task<Result<UploadDocumentResponse>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            return Result<UploadDocumentResponse>.Failure(new[] { "File is required." });
        }

        // Check file size (25 MB limit)
        const long maxFileSize = 25 * 1024 * 1024; // 25 MB
        if (request.File.Length > maxFileSize)
        {
            return Result<UploadDocumentResponse>.Failure(new[] { "File size exceeds 25 MB limit." });
        }

        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<UploadDocumentResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<UploadDocumentResponse>.Failure(new[] { "No workspace selected." });
        }

        // Get project with members and organization
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.Organization)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result<UploadDocumentResponse>.Failure(new[] { "Project not found." });
        }

        // Check if user has permission (Owner or Contributor)
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null || userMember.Role == ProjectRole.Viewer)
        {
            return Result<UploadDocumentResponse>.Failure(new[] { "You do not have permission to upload documents to this project." });
        }

        // Check storage limits
        if (project.Organization.MaxStorageGb.HasValue)
        {
            var maxStorageBytes = project.Organization.MaxStorageGb.Value * 1024L * 1024L * 1024L;
            var newTotalStorage = project.Organization.StorageUsedBytes + request.File.Length;

            if (newTotalStorage > maxStorageBytes)
            {
                return Result<UploadDocumentResponse>.Failure(new[]
                {
                    $"Storage limit exceeded. Your plan allows {project.Organization.MaxStorageGb}GB."
                });
            }
        }

        // Upload file to storage
        string storagePath;
        string fileHash;
        using (var stream = request.File.OpenReadStream())
        {
            storagePath = await _fileStorageService.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                organizationId.Value,
                project.Id,
                cancellationToken
            );

            // Compute file hash
            stream.Position = 0;
            fileHash = await _fileStorageService.ComputeFileHashAsync(stream, cancellationToken);
        }

        // Parse document if supported
        string? extractedText = null;
        string? extractionMethod = null;
        bool isTextExtracted = false;
        DateTime? textExtractedDate = null;
        int? pageCount = null;

        if (_documentParsingService.IsSupportedContentType(request.File.ContentType))
        {
            try
            {
                using var parseStream = request.File.OpenReadStream();
                var parseResult = await _documentParsingService.ParseDocumentAsync(
                    parseStream,
                    request.File.ContentType,
                    cancellationToken
                );

                if (parseResult.IsSuccessful)
                {
                    extractedText = parseResult.ExtractedText;
                    extractionMethod = parseResult.ExtractionMethod;
                    isTextExtracted = true;
                    textExtractedDate = DateTime.UtcNow;
                    pageCount = parseResult.PageCount;
                }
            }
            catch (Exception)
            {
                // Parsing failed, but continue with upload
                isTextExtracted = false;
            }
        }

        // Determine category
        var category = request.Category ?? DocumentCategory.Other;

        // Get user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        var userName = user is IgnaCheck.Infrastructure.Identity.ApplicationUser appUser
            ? $"{appUser.FirstName} {appUser.LastName}".Trim()
            : "Unknown User";

        // Create document entity
        var document = new Document
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            FileName = request.File.FileName,
            DisplayName = Path.GetFileNameWithoutExtension(request.File.FileName),
            Description = request.Description,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            StoragePath = storagePath,
            FileHash = fileHash,
            Category = category,
            Tags = request.Tags,
            ExtractedText = extractedText,
            ExtractionMethod = extractionMethod,
            IsTextExtracted = isTextExtracted,
            TextExtractedDate = textExtractedDate,
            PageCount = pageCount,
            Version = 1,
            IsLatestVersion = true,
            UploadedDate = DateTime.UtcNow,
            UploadedBy = userName,
            IsAnalyzed = false
        };

        _context.Documents.Add(document);

        // Update organization storage usage
        project.Organization.StorageUsedBytes += request.File.Length;

        // Log activity
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            UserId = _currentUser.Id,
            UserName = userName,
            UserEmail = appUser?.Email ?? "unknown@example.com",
            ActivityType = ActivityType.DocumentUploaded,
            EntityType = "Document",
            EntityId = document.Id,
            EntityName = document.FileName,
            Description = $"Uploaded document '{document.FileName}' to project '{project.Name}'",
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UploadDocumentResponse
        {
            Id = document.Id,
            FileName = document.FileName,
            DisplayName = document.DisplayName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            Category = document.Category,
            IsTextExtracted = document.IsTextExtracted,
            UploadedDate = document.UploadedDate
        };

        return Result<UploadDocumentResponse>.Success(response);
    }
}
