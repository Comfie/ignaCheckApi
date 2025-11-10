using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Common.Models.AI;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Infrastructure.Identity;

namespace IgnaCheck.Application.Audit.Commands.RunAuditCheck;

/// <summary>
/// Command to initiate an AI-powered compliance audit check for a project and framework.
/// </summary>
public record RunAuditCheckCommand : IRequest<Result<AuditCheckResponse>>
{
    /// <summary>
    /// Project to audit.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Framework to audit against.
    /// </summary>
    public Guid FrameworkId { get; init; }

    /// <summary>
    /// Analysis options.
    /// </summary>
    public AnalysisOptions? Options { get; init; }
}

/// <summary>
/// Response from initiating an audit check.
/// </summary>
public record AuditCheckResponse
{
    public Guid AuditCheckId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid FrameworkId { get; init; }
    public string FrameworkName { get; init; } = string.Empty;
    public int TotalControls { get; init; }
    public int DocumentsAnalyzed { get; init; }
    public DateTime StartedAt { get; init; }
    public string Status { get; init; } = "Running";
}

/// <summary>
/// Handler for RunAuditCheckCommand.
/// </summary>
public class RunAuditCheckCommandHandler : IRequestHandler<RunAuditCheckCommand, Result<AuditCheckResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IAIAnalysisService _aiAnalysisService;
    private readonly IIdentityService _identityService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentParsingService _documentParsingService;

    public RunAuditCheckCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService,
        IAIAnalysisService aiAnalysisService,
        IIdentityService identityService,
        IFileStorageService fileStorageService,
        IDocumentParsingService documentParsingService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _aiAnalysisService = aiAnalysisService;
        _identityService = identityService;
        _fileStorageService = fileStorageService;
        _documentParsingService = documentParsingService;
    }

    public async Task<Result<AuditCheckResponse>> Handle(RunAuditCheckCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<AuditCheckResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<AuditCheckResponse>.Failure(new[] { "No workspace selected." });
        }

        // Get project with members, frameworks, and documents
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.ProjectFrameworks)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result<AuditCheckResponse>.Failure(new[] { "Project not found." });
        }

        // Check if user has permission (Owner or Contributor)
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null || userMember.Role == ProjectRole.Viewer)
        {
            return Result<AuditCheckResponse>.Failure(new[] { "You do not have permission to run audit checks on this project." });
        }

        // Verify framework is assigned to project
        var projectFramework = project.ProjectFrameworks
            .FirstOrDefault(pf => pf.FrameworkId == request.FrameworkId && pf.IsActive);

        if (projectFramework == null)
        {
            return Result<AuditCheckResponse>.Failure(new[] { "Framework is not assigned to this project." });
        }

        // Check if project has documents
        var documents = project.Documents.ToList();
        if (!documents.Any())
        {
            return Result<AuditCheckResponse>.Failure(new[] { "Project must have at least one document to analyze." });
        }

        // Get framework with controls
        var framework = await _context.ComplianceFrameworks
            .Include(f => f.Controls)
            .FirstOrDefaultAsync(f => f.Id == request.FrameworkId, cancellationToken);

        if (framework == null)
        {
            return Result<AuditCheckResponse>.Failure(new[] { "Framework not found." });
        }

        if (!framework.Controls.Any())
        {
            return Result<AuditCheckResponse>.Failure(new[] { "Framework has no controls defined." });
        }

        // Get user details for activity log
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<AuditCheckResponse>.Failure(new[] { "User not found." });
        }

        var userName = $"{appUser.FirstName} {appUser.LastName}".Trim();

        // Prepare document contents for analysis
        // Extract text on-demand if not cached in database
        var documentContents = new List<DocumentContent>();
        foreach (var doc in documents)
        {
            string textContent = doc.ExtractedText ?? string.Empty;

            // If text is not cached, extract it on-demand from file storage
            if (string.IsNullOrWhiteSpace(textContent) && _documentParsingService.IsSupportedContentType(doc.ContentType))
            {
                try
                {
                    // Retrieve file from storage
                    using var fileStream = await _fileStorageService.GetFileAsync(doc.StoragePath, cancellationToken);

                    // Extract text on-the-fly
                    var parseResult = await _documentParsingService.ParseDocumentAsync(
                        fileStream,
                        doc.ContentType,
                        cancellationToken);

                    if (parseResult.IsSuccessful)
                    {
                        textContent = parseResult.ExtractedText ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue - document will be processed with empty content
                    Console.WriteLine($"Failed to extract text on-demand for {doc.FileName}: {ex.Message}");
                }
            }

            documentContents.Add(new DocumentContent
            {
                DocumentId = doc.Id,
                FileName = doc.FileName,
                Content = textContent,
                MimeType = doc.ContentType,
                PageCount = doc.PageCount ?? 0
            });
        }

        // Prepare framework analysis request
        var analysisRequest = new FrameworkAnalysisRequest
        {
            ProjectId = project.Id,
            FrameworkId = framework.Id,
            FrameworkCode = framework.Code,
            FrameworkName = framework.Name,
            Controls = framework.Controls.Select(c => new ControlSummary
            {
                ControlId = c.Id,
                ControlCode = c.ControlCode,
                Title = c.Title,
                Description = c.Description,
                ImplementationGuidance = c.ImplementationGuidance,
                DefaultRiskLevel = c.DefaultRiskLevel,
                IsMandatory = c.IsMandatory
            }).ToList(),
            Documents = documentContents,
            Options = request.Options ?? new AnalysisOptions()
        };

        // Log activity - audit check started
        var auditCheckId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;

        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            UserId = _currentUser.Id,
            UserName = userName,
            UserEmail = appUser.Email!,
            ActivityType = ActivityType.ComplianceCheckStarted,
            EntityType = "AuditCheck",
            EntityId = auditCheckId,
            EntityName = $"{framework.Name} Audit",
            Description = $"Started compliance check for framework '{framework.Name}' on project '{project.Name}'",
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                FrameworkId = framework.Id,
                FrameworkCode = framework.Code,
                ControlCount = framework.Controls.Count,
                DocumentCount = documents.Count
            }),
            OccurredAt = startTime
        };

        _context.ActivityLogs.Add(activityLog);

        // Run AI analysis
        var analysisResult = await _aiAnalysisService.AnalyzeDocumentsAgainstFrameworkAsync(
            analysisRequest,
            null, // No progress callback for now - can be enhanced later with SignalR
            cancellationToken);

        // Create findings from analysis results
        var findingsCreated = 0;
        foreach (var result in analysisResult.Results)
        {
            // Skip if compliant or not applicable
            if (result.Status == ComplianceStatus.Compliant || result.Status == ComplianceStatus.NotApplicable)
            {
                continue;
            }

            var finding = new ComplianceFinding
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId.Value,
                ProjectId = project.Id,
                ControlId = result.ControlId,
                FindingCode = $"{framework.Code}-{result.ControlId:N}".Substring(0, 20),
                Title = result.FindingTitle,
                Description = result.FindingDescription,
                Status = result.Status,
                RiskLevel = result.RiskLevel,
                RemediationGuidance = result.RemediationGuidance,
                EstimatedEffort = result.EstimatedEffortHours,
                ConfidenceScore = result.ConfidenceScore,
                LastAnalysisDate = startTime,
                AnalysisVersion = 1,
                AnalysisModel = analysisRequest.Options.Model,
                RawAnalysisData = System.Text.Json.JsonSerializer.Serialize(result)
            };

            _context.ComplianceFindings.Add(finding);

            // Create evidence references
            foreach (var evidenceRef in result.EvidenceReferences)
            {
                var evidence = new FindingEvidence
                {
                    Id = Guid.NewGuid(),
                    FindingId = finding.Id,
                    DocumentId = evidenceRef.DocumentId,
                    EvidenceType = (Domain.Entities.EvidenceType)(int)evidenceRef.EvidenceType,
                    Excerpt = evidenceRef.Excerpt,
                    PageReference = evidenceRef.PageReference,
                    RelevanceScore = evidenceRef.RelevanceScore,
                    IsManuallyAdded = false
                };

                _context.FindingEvidence.Add(evidence);
            }

            findingsCreated++;
        }

        // Update project framework statistics
        projectFramework.LastAnalysisDate = analysisResult.AnalysisCompleted;
        projectFramework.LastAnalysisBy = userName;
        projectFramework.CompliancePercentage = analysisResult.Summary.OverallComplianceScore;
        projectFramework.TotalControlsCount = analysisResult.TotalControls;
        projectFramework.CompliantControlsCount = analysisResult.Summary.CompliantCount;
        projectFramework.PartiallyCompliantControlsCount = analysisResult.Summary.PartiallyCompliantCount;
        projectFramework.NonCompliantControlsCount = analysisResult.Summary.NonCompliantCount;
        projectFramework.NotAssessedControlsCount = analysisResult.Summary.NotAssessedCount;
        projectFramework.Status = analysisResult.Summary.OverallComplianceScore >= 90 ? ComplianceStatus.Compliant :
                                  analysisResult.Summary.OverallComplianceScore >= 50 ? ComplianceStatus.PartiallyCompliant :
                                  ComplianceStatus.NonCompliant;

        // Log completion
        var completionLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            UserId = _currentUser.Id,
            UserName = userName,
            UserEmail = appUser.Email!,
            ActivityType = ActivityType.ComplianceCheckCompleted,
            EntityType = "AuditCheck",
            EntityId = auditCheckId,
            EntityName = $"{framework.Name} Audit",
            Description = $"Completed compliance check for framework '{framework.Name}'. Found {findingsCreated} gaps.",
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                FrameworkId = framework.Id,
                Duration = analysisResult.Duration.TotalSeconds,
                FindingsCreated = findingsCreated,
                ComplianceScore = analysisResult.Summary.OverallComplianceScore,
                Summary = analysisResult.Summary
            }),
            OccurredAt = analysisResult.AnalysisCompleted
        };

        _context.ActivityLogs.Add(completionLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuditCheckResponse>.Success(new AuditCheckResponse
        {
            AuditCheckId = auditCheckId,
            ProjectId = project.Id,
            FrameworkId = framework.Id,
            FrameworkName = framework.Name,
            TotalControls = analysisResult.TotalControls,
            DocumentsAnalyzed = documents.Count,
            StartedAt = startTime,
            Status = "Completed"
        });
    }
}
