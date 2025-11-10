using IgnaCheck.Application.Administration.Queries.GetAuditLogs;
using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Workspaces.Commands.DeleteWorkspace;
using IgnaCheck.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for workspace administration operations.
/// Only accessible by workspace owners and admins.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdministrationController : ApiControllerBase
{
    private readonly ISender _sender;

    public AdministrationController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get workspace audit logs with filtering.
    /// Only accessible by workspace owners and admins.
    /// </summary>
    /// <param name="activityType">Filter by activity type (optional)</param>
    /// <param name="userId">Filter by user ID (optional)</param>
    /// <param name="entityType">Filter by entity type (optional)</param>
    /// <param name="entityId">Filter by entity ID (optional)</param>
    /// <param name="startDate">Start date for date range (optional)</param>
    /// <param name="endDate">End date for date range (optional)</param>
    /// <param name="searchTerm">Search term for description (optional)</param>
    /// <param name="limit">Number of records to return (default: 100, max: 1000)</param>
    /// <param name="offset">Offset for pagination (default: 0)</param>
    /// <returns>Audit logs with pagination info</returns>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(Result<AuditLogsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<AuditLogsResultDto>>> GetAuditLogs(
        [FromQuery] ActivityType? activityType = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0)
    {
        var query = new GetAuditLogsQuery
        {
            ActivityType = activityType,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            StartDate = startDate,
            EndDate = endDate,
            SearchTerm = searchTerm,
            Limit = limit,
            Offset = offset
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Export audit logs to CSV format.
    /// Only accessible by workspace owners and admins.
    /// </summary>
    /// <param name="activityType">Filter by activity type (optional)</param>
    /// <param name="userId">Filter by user ID (optional)</param>
    /// <param name="entityType">Filter by entity type (optional)</param>
    /// <param name="entityId">Filter by entity ID (optional)</param>
    /// <param name="startDate">Start date for date range (optional)</param>
    /// <param name="endDate">End date for date range (optional)</param>
    /// <param name="searchTerm">Search term for description (optional)</param>
    /// <returns>CSV file with audit logs</returns>
    [HttpGet("audit-logs/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ExportAuditLogs(
        [FromQuery] ActivityType? activityType = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAuditLogsQuery
        {
            ActivityType = activityType,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            StartDate = startDate,
            EndDate = endDate,
            SearchTerm = searchTerm,
            Limit = 10000, // Export more records than normal queries
            Offset = 0
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        // Generate CSV
        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,ActivityType,Description,UserName,UserId,EntityType,EntityId,EntityName");

        foreach (var log in result.Data!.Logs)
        {
            csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.ActivityType}\",\"{EscapeCsv(log.Description)}\",\"{EscapeCsv(log.UserName)}\",\"{log.UserId}\",\"{log.EntityType ?? ""}\",\"{log.EntityId?.ToString() ?? ""}\",\"{EscapeCsv(log.EntityName ?? "")}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Permanently delete the workspace and all associated data.
    /// This is a destructive operation that cannot be undone.
    /// Only accessible by workspace owners.
    /// </summary>
    /// <param name="command">Delete workspace command with confirmation</param>
    /// <returns>Success result</returns>
    [HttpDelete("workspace")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> DeleteWorkspace([FromBody] DeleteWorkspaceCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape double quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}
