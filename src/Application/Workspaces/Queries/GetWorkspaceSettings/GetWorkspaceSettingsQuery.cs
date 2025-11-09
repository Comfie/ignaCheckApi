using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Workspaces.Commands.UpdateWorkspaceSettings;

namespace IgnaCheck.Application.Workspaces.Queries.GetWorkspaceSettings;

/// <summary>
/// Query to get workspace settings.
/// </summary>
public record GetWorkspaceSettingsQuery : IRequest<Result<WorkspaceSettingsDto>>
{
    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }
}

/// <summary>
/// Handler for the GetWorkspaceSettingsQuery.
/// </summary>
public class GetWorkspaceSettingsQueryHandler : IRequestHandler<GetWorkspaceSettingsQuery, Result<WorkspaceSettingsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetWorkspaceSettingsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceSettingsDto>> Handle(GetWorkspaceSettingsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<WorkspaceSettingsDto>.Failure(new[] { "User must be authenticated." });
        }

        // Determine workspace ID
        var workspaceId = request.WorkspaceId ?? _currentUser.OrganizationId;
        if (!workspaceId.HasValue)
        {
            return Result<WorkspaceSettingsDto>.Failure(new[] { "Workspace ID is required." });
        }

        // Get organization
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == workspaceId.Value, cancellationToken);

        if (organization == null)
        {
            return Result<WorkspaceSettingsDto>.Failure(new[] { "Workspace not found." });
        }

        // Check if user is a member of this workspace
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (membership == null)
        {
            return Result<WorkspaceSettingsDto>.Failure(new[] { "You are not a member of this workspace." });
        }

        var response = new WorkspaceSettingsDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            Slug = organization.Slug,
            LogoUrl = organization.LogoUrl,
            SubscriptionTier = organization.SubscriptionTier,
            TrialEndsAt = organization.TrialEndsAt,
            IsActive = organization.IsActive,
            BillingEmail = organization.BillingEmail,
            CompanyName = organization.CompanyName,
            TaxId = organization.TaxId,
            BillingAddress = organization.BillingAddress,
            DefaultCurrency = organization.DefaultCurrency,
            DefaultTimezone = organization.DefaultTimezone,
            DefaultLanguage = organization.DefaultLanguage,
            MaxMembers = organization.MaxMembers,
            MaxProjects = organization.MaxProjects,
            MaxStorageGB = organization.MaxStorageGB,
            CreatedDate = organization.Created,
            LastModifiedDate = organization.LastModified
        };

        return Result<WorkspaceSettingsDto>.Success(response);
    }
}
