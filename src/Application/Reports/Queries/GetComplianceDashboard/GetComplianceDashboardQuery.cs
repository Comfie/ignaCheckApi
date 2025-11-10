using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Reports.Queries.GetComplianceDashboard;

/// <summary>
/// Query to get high-level compliance dashboard for a project.
/// </summary>
public record GetComplianceDashboardQuery : IRequest<Result<ComplianceDashboardDto>>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }
}

/// <summary>
/// Compliance dashboard data.
/// </summary>
public record ComplianceDashboardDto
{
    /// <summary>
    /// Overall compliance score (0-100).
    /// </summary>
    public decimal OverallComplianceScore { get; init; }

    /// <summary>
    /// Total number of controls assessed.
    /// </summary>
    public int TotalControls { get; init; }

    /// <summary>
    /// Number of compliant controls.
    /// </summary>
    public int CompliantControls { get; init; }

    /// <summary>
    /// Number of partially compliant controls.
    /// </summary>
    public int PartiallyCompliantControls { get; init; }

    /// <summary>
    /// Number of non-compliant controls.
    /// </summary>
    public int NonCompliantControls { get; init; }

    /// <summary>
    /// Number of not assessed controls.
    /// </summary>
    public int NotAssessedControls { get; init; }

    /// <summary>
    /// Findings distribution by severity.
    /// </summary>
    public FindingsBySeverityDto FindingsBySeverity { get; init; } = new();

    /// <summary>
    /// Findings distribution by workflow status.
    /// </summary>
    public FindingsByWorkflowStatusDto FindingsByWorkflowStatus { get; init; } = new();

    /// <summary>
    /// Compliance breakdown by framework.
    /// </summary>
    public List<FrameworkComplianceDto> FrameworkBreakdown { get; init; } = new();

    /// <summary>
    /// Trend data over time (last 6 analysis runs).
    /// </summary>
    public List<ComplianceTrendDto> ComplianceTrend { get; init; } = new();

    /// <summary>
    /// Top priority findings (highest risk, unresolved).
    /// </summary>
    public List<TopFindingDto> TopPriorityFindings { get; init; } = new();
}

public record FindingsBySeverityDto
{
    public int Critical { get; init; }
    public int High { get; init; }
    public int Medium { get; init; }
    public int Low { get; init; }
}

public record FindingsByWorkflowStatusDto
{
    public int Open { get; init; }
    public int InProgress { get; init; }
    public int Resolved { get; init; }
    public int Accepted { get; init; }
    public int FalsePositive { get; init; }
}

public record FrameworkComplianceDto
{
    public Guid FrameworkId { get; init; }
    public string FrameworkName { get; init; } = string.Empty;
    public string FrameworkCode { get; init; } = string.Empty;
    public decimal ComplianceScore { get; init; }
    public int TotalControls { get; init; }
    public int CompliantControls { get; init; }
    public int TotalFindings { get; init; }
    public int CriticalFindings { get; init; }
}

public record ComplianceTrendDto
{
    public DateTime AnalysisDate { get; init; }
    public decimal ComplianceScore { get; init; }
    public int TotalFindings { get; init; }
}

public record TopFindingDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public RiskLevel RiskLevel { get; init; }
    public string FrameworkName { get; init; } = string.Empty;
    public string ControlCode { get; init; } = string.Empty;
    public FindingWorkflowStatus WorkflowStatus { get; init; }
    public string? AssignedTo { get; init; }
}

/// <summary>
/// Handler for GetComplianceDashboardQuery.
/// </summary>
public class GetComplianceDashboardQueryHandler : IRequestHandler<GetComplianceDashboardQuery, Result<ComplianceDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetComplianceDashboardQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ComplianceDashboardDto>> Handle(GetComplianceDashboardQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<ComplianceDashboardDto>.Failure(new[] { "User must be authenticated." });
        }

        // Verify project exists and user has access
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.ProjectFrameworks)
            .ThenInclude(pf => pf.Framework)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result<ComplianceDashboardDto>.Failure(new[] { "Project not found." });
        }

        // Check if user is a member of the project
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (!isMember)
        {
            return Result<ComplianceDashboardDto>.Failure(new[] { "Access denied. You are not a member of this project." });
        }

        // Get all findings for the project
        var findings = await _context.ComplianceFindings
            .Include(f => f.Control)
            .ThenInclude(c => c.Framework)
            .Where(f => f.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);

        // Calculate overall compliance statistics
        var statusGroups = findings.GroupBy(f => f.Status).ToDictionary(g => g.Key, g => g.Count());
        var totalControls = findings.Count;
        var compliantControls = statusGroups.GetValueOrDefault(ComplianceStatus.Compliant, 0);
        var partiallyCompliant = statusGroups.GetValueOrDefault(ComplianceStatus.PartiallyCompliant, 0);
        var nonCompliant = statusGroups.GetValueOrDefault(ComplianceStatus.NonCompliant, 0);
        var notAssessed = statusGroups.GetValueOrDefault(ComplianceStatus.NotAssessed, 0);

        // Calculate overall compliance score (weighted: Compliant=100%, PartiallyCompliant=50%, NonCompliant=0%, NotAssessed=0%)
        var overallScore = totalControls > 0
            ? (decimal)(compliantControls * 100 + partiallyCompliant * 50) / totalControls
            : 0;

        // Findings by severity
        var severityGroups = findings.GroupBy(f => f.RiskLevel).ToDictionary(g => g.Key, g => g.Count());
        var findingsBySeverity = new FindingsBySeverityDto
        {
            Critical = severityGroups.GetValueOrDefault(RiskLevel.Critical, 0),
            High = severityGroups.GetValueOrDefault(RiskLevel.High, 0),
            Medium = severityGroups.GetValueOrDefault(RiskLevel.Medium, 0),
            Low = severityGroups.GetValueOrDefault(RiskLevel.Low, 0)
        };

        // Findings by workflow status
        var workflowGroups = findings.GroupBy(f => f.WorkflowStatus).ToDictionary(g => g.Key, g => g.Count());
        var findingsByWorkflowStatus = new FindingsByWorkflowStatusDto
        {
            Open = workflowGroups.GetValueOrDefault(FindingWorkflowStatus.Open, 0),
            InProgress = workflowGroups.GetValueOrDefault(FindingWorkflowStatus.InProgress, 0),
            Resolved = workflowGroups.GetValueOrDefault(FindingWorkflowStatus.Resolved, 0),
            Accepted = workflowGroups.GetValueOrDefault(FindingWorkflowStatus.Accepted, 0),
            FalsePositive = workflowGroups.GetValueOrDefault(FindingWorkflowStatus.FalsePositive, 0)
        };

        // Framework breakdown
        var frameworkBreakdown = findings
            .GroupBy(f => new { f.Control.FrameworkId, f.Control.Framework.Name, f.Control.Framework.Code })
            .Select(g =>
            {
                var frameworkFindings = g.ToList();
                var frameworkCompliant = frameworkFindings.Count(f => f.Status == ComplianceStatus.Compliant);
                var frameworkPartial = frameworkFindings.Count(f => f.Status == ComplianceStatus.PartiallyCompliant);
                var frameworkTotal = frameworkFindings.Count;
                var frameworkScore = frameworkTotal > 0
                    ? (decimal)(frameworkCompliant * 100 + frameworkPartial * 50) / frameworkTotal
                    : 0;

                return new FrameworkComplianceDto
                {
                    FrameworkId = g.Key.FrameworkId,
                    FrameworkName = g.Key.Name,
                    FrameworkCode = g.Key.Code,
                    ComplianceScore = frameworkScore,
                    TotalControls = frameworkTotal,
                    CompliantControls = frameworkCompliant,
                    TotalFindings = frameworkFindings.Count,
                    CriticalFindings = frameworkFindings.Count(f => f.RiskLevel == RiskLevel.Critical)
                };
            })
            .OrderByDescending(f => f.CriticalFindings)
            .ThenBy(f => f.ComplianceScore)
            .ToList();

        // Compliance trend (last 6 unique analysis dates)
        var complianceTrend = findings
            .Where(f => f.LastAnalysisDate.HasValue)
            .GroupBy(f => f.LastAnalysisDate!.Value.Date)
            .OrderByDescending(g => g.Key)
            .Take(6)
            .Select(g =>
            {
                var trendFindings = g.ToList();
                var trendCompliant = trendFindings.Count(f => f.Status == ComplianceStatus.Compliant);
                var trendPartial = trendFindings.Count(f => f.Status == ComplianceStatus.PartiallyCompliant);
                var trendTotal = trendFindings.Count;
                var trendScore = trendTotal > 0
                    ? (decimal)(trendCompliant * 100 + trendPartial * 50) / trendTotal
                    : 0;

                return new ComplianceTrendDto
                {
                    AnalysisDate = g.Key,
                    ComplianceScore = trendScore,
                    TotalFindings = trendTotal
                };
            })
            .OrderBy(t => t.AnalysisDate)
            .ToList();

        // Top priority findings (highest risk, unresolved)
        var topPriorityFindings = findings
            .Where(f => f.WorkflowStatus != FindingWorkflowStatus.Resolved && f.WorkflowStatus != FindingWorkflowStatus.FalsePositive)
            .OrderByDescending(f => f.RiskLevel)
            .ThenBy(f => f.DueDate ?? DateTime.MaxValue)
            .Take(10)
            .Select(f => new TopFindingDto
            {
                Id = f.Id,
                Title = f.Title,
                RiskLevel = f.RiskLevel,
                FrameworkName = f.Control.Framework.Name,
                ControlCode = f.Control.ControlCode,
                WorkflowStatus = f.WorkflowStatus,
                AssignedTo = f.AssignedTo
            })
            .ToList();

        var dashboard = new ComplianceDashboardDto
        {
            OverallComplianceScore = overallScore,
            TotalControls = totalControls,
            CompliantControls = compliantControls,
            PartiallyCompliantControls = partiallyCompliant,
            NonCompliantControls = nonCompliant,
            NotAssessedControls = notAssessed,
            FindingsBySeverity = findingsBySeverity,
            FindingsByWorkflowStatus = findingsByWorkflowStatus,
            FrameworkBreakdown = frameworkBreakdown,
            ComplianceTrend = complianceTrend,
            TopPriorityFindings = topPriorityFindings
        };

        return Result<ComplianceDashboardDto>.Success(dashboard);
    }
}
