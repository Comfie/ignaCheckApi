using IgnaCheck.Application.Activities.Queries.GetProjectActivity;
using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Reports.Queries.GetComplianceDashboard;
using IgnaCheck.Application.Reports.Queries.GetExecutiveSummary;
using IgnaCheck.Application.Reports.Queries.GetFrameworkReport;
using IgnaCheck.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for compliance reports and analytics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ApiControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get high-level compliance dashboard for a project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Compliance dashboard</returns>
    [HttpGet("projects/{projectId}/dashboard")]
    [ProducesResponseType(typeof(Result<ComplianceDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ComplianceDashboardDto>>> GetComplianceDashboard(Guid projectId)
    {
        var query = new GetComplianceDashboardQuery { ProjectId = projectId };
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get detailed compliance report for a specific framework.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="frameworkId">Framework ID</param>
    /// <returns>Framework compliance report</returns>
    [HttpGet("projects/{projectId}/frameworks/{frameworkId}")]
    [ProducesResponseType(typeof(Result<FrameworkReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<FrameworkReportDto>>> GetFrameworkReport(Guid projectId, Guid frameworkId)
    {
        var query = new GetFrameworkReportQuery
        {
            ProjectId = projectId,
            FrameworkId = frameworkId
        };
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get executive summary report for a project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Executive summary</returns>
    [HttpGet("projects/{projectId}/executive-summary")]
    [ProducesResponseType(typeof(Result<ExecutiveSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ExecutiveSummaryDto>>> GetExecutiveSummary(Guid projectId)
    {
        var query = new GetExecutiveSummaryQuery { ProjectId = projectId };
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get audit trail report for a project (activity log).
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="activityType">Filter by activity type (optional)</param>
    /// <param name="userId">Filter by user ID (optional)</param>
    /// <param name="startDate">Start date for date range (optional)</param>
    /// <param name="endDate">End date for date range (optional)</param>
    /// <param name="limit">Number of records to return (default: 100, max: 500)</param>
    /// <returns>Activity log entries</returns>
    [HttpGet("projects/{projectId}/audit-trail")]
    [ProducesResponseType(typeof(Result<List<ActivityLogDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<List<ActivityLogDto>>>> GetAuditTrail(
        Guid projectId,
        [FromQuery] ActivityType? activityType = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int limit = 100)
    {
        var query = new GetProjectActivityQuery
        {
            ProjectId = projectId,
            ActivityType = activityType,
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            Limit = limit
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Export compliance dashboard as PDF (placeholder - to be implemented with PDF generation library).
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>PDF file</returns>
    [HttpGet("projects/{projectId}/dashboard/export/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public Task<ActionResult> ExportDashboardPdf(Guid projectId)
    {
        // TODO: Implement PDF export using a library like QuestPDF or iTextSharp
        // For now, return not implemented
        return Task.FromResult<ActionResult>(StatusCode(StatusCodes.Status501NotImplemented,
            Result.Failure(new[] { "PDF export functionality will be implemented in a future release." })));
    }

    /// <summary>
    /// Export framework report as Excel (placeholder - to be implemented with Excel generation library).
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="frameworkId">Framework ID</param>
    /// <returns>Excel file</returns>
    [HttpGet("projects/{projectId}/frameworks/{frameworkId}/export/excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public Task<ActionResult> ExportFrameworkReportExcel(Guid projectId, Guid frameworkId)
    {
        // TODO: Implement Excel export using EPPlus or ClosedXML
        // For now, return not implemented
        return Task.FromResult<ActionResult>(StatusCode(StatusCodes.Status501NotImplemented,
            Result.Failure(new[] { "Excel export functionality will be implemented in a future release." })));
    }

    /// <summary>
    /// Export executive summary as PDF (placeholder - to be implemented with PDF generation library).
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>PDF file</returns>
    [HttpGet("projects/{projectId}/executive-summary/export/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public Task<ActionResult> ExportExecutiveSummaryPdf(Guid projectId)
    {
        // TODO: Implement PDF export
        // For now, return not implemented
        return Task.FromResult<ActionResult>(StatusCode(StatusCodes.Status501NotImplemented,
            Result.Failure(new[] { "PDF export functionality will be implemented in a future release." })));
    }
}
