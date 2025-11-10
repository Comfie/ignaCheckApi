using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Workspaces.Commands.CreateWorkspace;

/// <summary>
/// Command to create a new workspace (organization).
/// </summary>
public record CreateWorkspaceCommand : IRequest<Result<CreateWorkspaceResponse>>
{
    /// <summary>
    /// Workspace name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional workspace description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional custom slug (URL identifier).
    /// </summary>
    public string? Slug { get; init; }

    /// <summary>
    /// Subscription tier (Free, Pro, Enterprise).
    /// </summary>
    public string? SubscriptionTier { get; init; }

    /// <summary>
    /// Billing email address.
    /// </summary>
    public string? BillingEmail { get; init; }

    // TODO: CompanyName should be stored in Organization.Settings JSON field
    // /// <summary>
    // /// Company name for billing.
    // /// </summary>
    // public string? CompanyName { get; init; }
}

/// <summary>
/// Response for successful workspace creation.
/// </summary>
public record CreateWorkspaceResponse
{
    /// <summary>
    /// Workspace ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Workspace name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Workspace slug (URL identifier).
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Subscription tier.
    /// </summary>
    public string? SubscriptionTier { get; init; }

    /// <summary>
    /// Trial end date (if applicable).
    /// </summary>
    public DateTime? TrialEndsAt { get; init; }

    /// <summary>
    /// Current user's role in the workspace.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// JWT access token with updated organization context.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the CreateWorkspaceCommand.
/// </summary>
public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<CreateWorkspaceResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IIdentityService _identityService;

    public CreateWorkspaceCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IJwtTokenGenerator jwtTokenGenerator,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityService = identityService;
    }

    public async Task<Result<CreateWorkspaceResponse>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<CreateWorkspaceResponse>.Failure(new[] { "User must be authenticated to create a workspace." });
        }

        // Get user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<CreateWorkspaceResponse>.Failure(new[] { "User not found." });
        }

        // Generate slug if not provided
        var slug = request.Slug;
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = GenerateSlug(request.Name);
        }
        else
        {
            slug = NormalizeSlug(slug);
        }

        // Ensure slug is unique
        var existingSlug = await _context.Organizations
            .AnyAsync(o => o.Slug == slug, cancellationToken);

        if (existingSlug)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        // Determine subscription tier and trial settings
        var subscriptionTier = request.SubscriptionTier ?? "Free";
        DateTime? trialEndsAt = null;

        if (subscriptionTier == "Free")
        {
            trialEndsAt = DateTime.UtcNow.AddDays(14); // 14-day trial for free tier
        }

        // Create organization
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            SubscriptionTier = subscriptionTier,
            TrialEndsAt = trialEndsAt,
            IsActive = true,
            BillingEmail = request.BillingEmail,
            // TODO: Store CompanyName in Organization.Settings JSON field
            // CompanyName = request.CompanyName,
            // Set resource limits based on tier
            MaxMembers = subscriptionTier switch
            {
                "Free" => 5,
                "Pro" => 50,
                "Enterprise" => null, // Unlimited
                _ => 5
            },
            MaxProjects = subscriptionTier switch
            {
                "Free" => 3,
                "Pro" => 50,
                "Enterprise" => null, // Unlimited
                _ => 3
            },
            MaxStorageGb = subscriptionTier switch
            {
                "Free" => 1,
                "Pro" => 100,
                "Enterprise" => 1000,
                _ => 1
            }
        };

        _context.Organizations.Add(organization);

        // Add current user as Owner
        var member = new OrganizationMember
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserId = _currentUser.Id,
            Role = Domain.Constants.WorkspaceRoles.Owner,
            JoinedDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.OrganizationMembers.Add(member);

        await _context.SaveChangesAsync(cancellationToken);

        // Get user roles for JWT
        var roles = new List<string>(); // TODO: Load actual user roles

        // Generate new JWT token with organization context
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            userId: _currentUser.Id,
            email: appUser.Email!,
            firstName: appUser.FirstName,
            lastName: appUser.LastName,
            roles: roles,
            organizationId: organization.Id,
            organizationRole: member.Role,
            expiresInMinutes: 60
        );

        var response = new CreateWorkspaceResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Slug = organization.Slug!,
            SubscriptionTier = organization.SubscriptionTier,
            TrialEndsAt = organization.TrialEndsAt,
            Role = member.Role,
            AccessToken = accessToken
        };

        return Result<CreateWorkspaceResponse>.Success(response);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
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
        slug = slug.Trim('-');

        // Add random suffix for uniqueness
        slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

        return slug;
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
