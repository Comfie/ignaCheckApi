using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Base controller for all API controllers.
/// Provides common functionality and configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ISender Sender => HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Gets the current user's ID from the JWT claims.
    /// </summary>
    protected string? CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Gets the current user's email from the JWT claims.
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    /// <summary>
    /// Gets the current organization/workspace ID from the JWT claims.
    /// </summary>
    protected Guid? CurrentOrganizationId
    {
        get
        {
            var orgIdClaim = User.FindFirst("organization_id")?.Value ?? User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(orgIdClaim, out var orgId) ? orgId : null;
        }
    }

    /// <summary>
    /// Gets the current user's role in the organization/workspace from the JWT claims.
    /// </summary>
    protected string? CurrentOrganizationRole => User.FindFirst("organization_role")?.Value ?? User.FindFirst("workspace_role")?.Value;

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
}
