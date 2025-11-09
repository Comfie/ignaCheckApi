using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Reports.Queries.GetFrameworkReport;

/// <summary>
/// Query to get detailed compliance report for a specific framework.
/// </summary>
public record GetFrameworkReportQuery : IRequest<Result<FrameworkReportDto>>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Framework ID.
    /// </summary>
    public Guid FrameworkId { get; init; }
}

/// <summary>
/// Framework-specific compliance report.
/// </summary>
public record FrameworkReportDto
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public Guid FrameworkId { get; init; }
    public string FrameworkName { get; init; } = string.Empty;
    public string FrameworkCode { get; init; } = string.Empty;
    public string? FrameworkVersion { get; init; }
    public DateTime ReportGeneratedDate { get; init; }

    // Summary statistics
    public decimal ComplianceScore { get; init; }
    public int TotalControls { get; init; }
    public int CompliantControls { get; init; }
    public int PartiallyCompliantControls { get; init; }
    public int NonCompliantControls { get; init; }
    public int NotAssessedControls { get; init; }

    // Findings summary
    public int TotalFindings { get; init; }
    public int CriticalFindings { get; init; }
    public int HighFindings { get; init; }
    public int MediumFindings { get; init; }
    public int LowFindings { get; init; }
    public int OpenFindings { get; init; }
    public int ResolvedFindings { get; init; }

    // Control details
    public List<ControlComplianceDto> Controls { get; init; } = new();
}

public record ControlComplianceDto
{
    public Guid ControlId { get; init; }
    public string ControlReference { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ComplianceStatus Status { get; init; }
    public List<FindingSummaryDto> Findings { get; init; } = new();
}

public record FindingSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public RiskLevel RiskLevel { get; init; }
    public FindingWorkflowStatus WorkflowStatus { get; init; }
    public string? AssignedTo { get; init; }
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// Handler for GetFrameworkReportQuery.
/// </summary>
public class GetFrameworkReportQueryHandler : IRequestHandler<GetFrameworkReportQuery, Result<FrameworkReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetFrameworkReportQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<FrameworkReportDto>> Handle(GetFrameworkReportQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<FrameworkReportDto>.Failure(new[] { "User must be authenticated." });
        }

        // Verify project exists and user has access
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result<FrameworkReportDto>.Failure(new[] { "Project not found." });
        }

        // Check if user is a member of the project
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (!isMember)
        {
            return Result<FrameworkReportDto>.Failure(new[] { "Access denied. You are not a member of this project." });
        }

        // Get framework
        var framework = await _context.ComplianceFrameworks
            .FirstOrDefaultAsync(f => f.Id == request.FrameworkId, cancellationToken);

        if (framework == null)
        {
            return Result<FrameworkReportDto>.Failure(new[] { "Framework not found." });
        }

        // Get all controls for this framework
        var controls = await _context.ComplianceControls
            .Where(c => c.FrameworkId == request.FrameworkId)
            .OrderBy(c => c.ControlReference)
            .ToListAsync(cancellationToken);

        // Get all findings for this framework and project
        var findings = await _context.ComplianceFindings
            .Include(f => f.Control)
            .Where(f => f.ProjectId == request.ProjectId && f.Control.FrameworkId == request.FrameworkId)
            .ToListAsync(cancellationToken);

        // Group findings by control
        var findingsByControl = findings.GroupBy(f => f.ControlId).ToDictionary(g => g.Key, g => g.ToList());

        // Calculate statistics
        var totalControls = findings.Count;
        var statusGroups = findings.GroupBy(f => f.Status).ToDictionary(g => g.Key, g => g.Count());
        var compliantControls = statusGroups.GetValueOrDefault(ComplianceStatus.Compliant, 0);
        var partiallyCompliant = statusGroups.GetValueOrDefault(ComplianceStatus.PartiallyCompliant, 0);
        var nonCompliant = statusGroups.GetValueOrDefault(ComplianceStatus.NonCompliant, 0);
        var notAssessed = statusGroups.GetValueOrDefault(ComplianceStatus.NotAssessed, 0);

        var complianceScore = totalControls > 0
            ? (decimal)(compliantControls * 100 + partiallyCompliant * 50) / totalControls
            : 0;

        // Risk level statistics
        var riskGroups = findings.GroupBy(f => f.RiskLevel).ToDictionary(g => g.Key, g => g.Count());
        var criticalFindings = riskGroups.GetValueOrDefault(RiskLevel.Critical, 0);
        var highFindings = riskGroups.GetValueOrDefault(RiskLevel.High, 0);
        var mediumFindings = riskGroups.GetValueOrDefault(RiskLevel.Medium, 0);
        var lowFindings = riskGroups.GetValueOrDefault(RiskLevel.Low, 0);

        // Workflow status statistics
        var openFindings = findings.Count(f => f.WorkflowStatus == FindingWorkflowStatus.Open);
        var resolvedFindings = findings.Count(f => f.WorkflowStatus == FindingWorkflowStatus.Resolved);

        // Build control compliance details
        var controlComplianceList = controls.Select(control =>
        {
            var controlFindings = findingsByControl.GetValueOrDefault(control.Id, new List<Domain.Entities.ComplianceFinding>());

            // Determine control status (if there are findings, use the finding status; otherwise NotAssessed)
            var controlStatus = controlFindings.Any()
                ? controlFindings.First().Status
                : ComplianceStatus.NotAssessed;

            return new ControlComplianceDto
            {
                ControlId = control.Id,
                ControlReference = control.ControlReference,
                Title = control.Title,
                Description = control.Description,
                Status = controlStatus,
                Findings = controlFindings.Select(f => new FindingSummaryDto
                {
                    Id = f.Id,
                    Title = f.Title,
                    RiskLevel = f.RiskLevel,
                    WorkflowStatus = f.WorkflowStatus,
                    AssignedTo = f.AssignedTo,
                    DueDate = f.DueDate
                }).ToList()
            };
        }).ToList();

        var report = new FrameworkReportDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            FrameworkId = framework.Id,
            FrameworkName = framework.Name,
            FrameworkCode = framework.Code,
            FrameworkVersion = framework.Version,
            ReportGeneratedDate = DateTime.UtcNow,
            ComplianceScore = complianceScore,
            TotalControls = totalControls,
            CompliantControls = compliantControls,
            PartiallyCompliantControls = partiallyCompliant,
            NonCompliantControls = nonCompliant,
            NotAssessedControls = notAssessed,
            TotalFindings = findings.Count,
            CriticalFindings = criticalFindings,
            HighFindings = highFindings,
            MediumFindings = mediumFindings,
            LowFindings = lowFindings,
            OpenFindings = openFindings,
            ResolvedFindings = resolvedFindings,
            Controls = controlComplianceList
        };

        return Result<FrameworkReportDto>.Success(report);
    }
}
