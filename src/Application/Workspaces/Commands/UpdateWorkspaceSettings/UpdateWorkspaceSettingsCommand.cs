using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Workspaces.Commands.UpdateWorkspaceSettings;

/// <summary>
/// Command to update workspace settings.
/// </summary>
public record UpdateWorkspaceSettingsCommand : IRequest<Result<WorkspaceSettingsDto>>
{
    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }

    /// <summary>
    /// Workspace name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Workspace description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Workspace slug (URL identifier).
    /// </summary>
    public string? Slug { get; init; }

    /// <summary>
    /// Workspace logo URL.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Billing email address.
    /// </summary>
    public string? BillingEmail { get; init; }

    /// <summary>
    /// Company name for billing.
    /// </summary>
    public string? CompanyName { get; init; }

    /// <summary>
    /// Tax ID for billing.
    /// </summary>
    public string? TaxId { get; init; }

    /// <summary>
    /// Billing address.
    /// </summary>
    public string? BillingAddress { get; init; }

    /// <summary>
    /// Default currency for billing.
    /// </summary>
    public string? DefaultCurrency { get; init; }

    /// <summary>
    /// Default timezone.
    /// </summary>
    public string? DefaultTimezone { get; init; }

    /// <summary>
    /// Default language.
    /// </summary>
    public string? DefaultLanguage { get; init; }
}

/// <summary>
/// Workspace settings DTO.
/// </summary>
public record WorkspaceSettingsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Slug { get; init; }
    public string? LogoUrl { get; init; }
    public string? SubscriptionTier { get; init; }
    public DateTime? TrialEndsAt { get; init; }
    public bool IsActive { get; init; }
    public string? BillingEmail { get; init; }
    public string? CompanyName { get; init; }
    public string? TaxId { get; init; }
    public string? BillingAddress { get; init; }
    public string? DefaultCurrency { get; init; }
    public string? DefaultTimezone { get; init; }
    public string? DefaultLanguage { get; init; }
    public int? MaxMembers { get; init; }
    public int? MaxProjects { get; init; }
    public int? MaxStorageGB { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? LastModifiedDate { get; init; }
}

/// <summary>
/// Handler for the UpdateWorkspaceSettingsCommand.
/// </summary>
public class UpdateWorkspaceSettingsCommandHandler : IRequestHandler<UpdateWorkspaceSettingsCommand, Result<WorkspaceSettingsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateWorkspaceSettingsCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceSettingsDto>> Handle(UpdateWorkspaceSettingsCommand request, CancellationToken cancellationToken)
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

        // Check if user has permission to update workspace settings (must be Owner or Admin)
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (membership == null)
        {
            return Result<WorkspaceSettingsDto>.Failure(new[] { "You are not a member of this workspace." });
        }

        if (membership.Role != Domain.Constants.WorkspaceRoles.Owner && membership.Role != Domain.Constants.WorkspaceRoles.Admin)
        {
            return Result<WorkspaceSettingsDto>.Failure(new[] { "Only workspace Owners and Admins can update workspace settings." });
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            organization.Name = request.Name;
        }

        if (request.Description != null)
        {
            organization.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = NormalizeSlug(request.Slug);

            // Check if slug is already in use by another workspace
            var existingSlug = await _context.Organizations
                .AnyAsync(o => o.Slug == normalizedSlug && o.Id != workspaceId.Value, cancellationToken);

            if (existingSlug)
            {
                return Result<WorkspaceSettingsDto>.Failure(new[] { "This slug is already in use by another workspace." });
            }

            organization.Slug = normalizedSlug;
        }

        if (request.LogoUrl != null)
        {
            organization.LogoUrl = request.LogoUrl;
        }

        if (request.BillingEmail != null)
        {
            organization.BillingEmail = request.BillingEmail;
        }

        if (request.CompanyName != null)
        {
            organization.CompanyName = request.CompanyName;
        }

        if (request.TaxId != null)
        {
            organization.TaxId = request.TaxId;
        }

        if (request.BillingAddress != null)
        {
            organization.BillingAddress = request.BillingAddress;
        }

        if (request.DefaultCurrency != null)
        {
            organization.DefaultCurrency = request.DefaultCurrency;
        }

        if (request.DefaultTimezone != null)
        {
            organization.DefaultTimezone = request.DefaultTimezone;
        }

        if (request.DefaultLanguage != null)
        {
            organization.DefaultLanguage = request.DefaultLanguage;
        }

        await _context.SaveChangesAsync(cancellationToken);

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

    private static string NormalizeSlug(string slug)
    {
        slug = slug.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove special characters
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remove consecutive dashes
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        // Trim dashes from start and end
        return slug.Trim('-');
    }
}
