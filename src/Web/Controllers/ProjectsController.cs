using IgnaCheck.Application.Projects.Commands.CreateProject;
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
}
