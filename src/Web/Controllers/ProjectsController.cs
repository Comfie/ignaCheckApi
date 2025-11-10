using IgnaCheck.Application.Activities.Queries.GetProjectActivity;
using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Projects.Commands.AddProjectMember;
using IgnaCheck.Application.Projects.Commands.ArchiveProject;
using IgnaCheck.Application.Projects.Commands.CreateProject;
using IgnaCheck.Application.Projects.Commands.DeleteProject;
using IgnaCheck.Application.Projects.Commands.RemoveProjectMember;
using IgnaCheck.Application.Projects.Commands.UpdateProject;
using IgnaCheck.Application.Projects.Commands.UpdateProjectMemberRole;
using IgnaCheck.Application.Projects.Queries.GetProjectDetails;
using IgnaCheck.Application.Projects.Queries.GetProjectsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for project management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ApiControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all projects accessible by the current user.
    /// </summary>
    /// <param name="status">Filter by project status (optional)</param>
    /// <param name="searchTerm">Search by name or description (optional)</param>
    /// <param name="myProjectsOnly">Show only my projects (default: false)</param>
    /// <param name="includeArchived">Include archived projects (default: false)</param>
    /// <returns>List of projects</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<ProjectDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<List<ProjectDto>>>> GetProjects(
        [FromQuery] Domain.Enums.ProjectStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool myProjectsOnly = false,
        [FromQuery] bool includeArchived = false)
    {
        var query = new GetProjectsListQuery
        {
            Status = status,
            SearchTerm = searchTerm,
            MyProjectsOnly = myProjectsOnly,
            IncludeArchived = includeArchived
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new project.
    /// </summary>
    /// <param name="command">Project creation details</param>
    /// <returns>Created project details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CreateProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<CreateProjectResponse>>> CreateProject([FromBody] CreateProjectCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about a specific project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<ProjectDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<ProjectDetailsDto>>> GetProjectDetails(Guid id)
    {
        var query = new GetProjectDetailsQuery { ProjectId = id };
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update an existing project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="command">Updated project details</param>
    /// <returns>Updated project details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<UpdateProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<UpdateProjectResponse>>> UpdateProject(Guid id, [FromBody] UpdateProjectCommand command)
    {
        if (id != command.ProjectId)
        {
            return BadRequest(Result.Failure(new[] { "Project ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a project permanently.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="command">Deletion confirmation</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> DeleteProject(Guid id, [FromBody] DeleteProjectCommand command)
    {
        if (id != command.ProjectId)
        {
            return BadRequest(Result.Failure(new[] { "Project ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Archive or restore a project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="command">Archive/restore command</param>
    /// <returns>Success result</returns>
    [HttpPost("{id}/archive")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> ArchiveProject(Guid id, [FromBody] ArchiveProjectCommand command)
    {
        if (id != command.ProjectId)
        {
            return BadRequest(Result.Failure(new[] { "Project ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add a member to a project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="command">Member addition details</param>
    /// <returns>Success result</returns>
    [HttpPost("{id}/members")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> AddProjectMember(Guid id, [FromBody] AddProjectMemberCommand command)
    {
        if (id != command.ProjectId)
        {
            return BadRequest(Result.Failure(new[] { "Project ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove a member from a project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="userId">User ID to remove</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id}/members/{userId}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> RemoveProjectMember(Guid id, string userId)
    {
        var command = new RemoveProjectMemberCommand { ProjectId = id, UserId = userId };
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a project member's role.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="command">Role update details</param>
    /// <returns>Success result</returns>
    [HttpPut("{id}/members/{userId}/role")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> UpdateProjectMemberRole(Guid id, string userId, [FromBody] UpdateProjectMemberRoleCommand command)
    {
        if (id != command.ProjectId || userId != command.UserId)
        {
            return BadRequest(Result.Failure(new[] { "Project ID or User ID mismatch." }));
        }

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get activity log for a project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="activityType">Filter by activity type (optional)</param>
    /// <param name="userId">Filter by user ID (optional)</param>
    /// <param name="startDate">Start date for date range (optional)</param>
    /// <param name="endDate">End date for date range (optional)</param>
    /// <param name="limit">Number of records to return (default: 100, max: 500)</param>
    /// <returns>Activity log entries</returns>
    [HttpGet("{id}/activity")]
    [ProducesResponseType(typeof(Result<List<ActivityLogDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<List<ActivityLogDto>>>> GetProjectActivity(
        Guid id,
        [FromQuery] Domain.Entities.ActivityType? activityType = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int limit = 100)
    {
        var query = new GetProjectActivityQuery
        {
            ProjectId = id,
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
}
