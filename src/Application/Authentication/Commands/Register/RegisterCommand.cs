using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Authentication.Commands.Register;

/// <summary>
/// Command to register a new user account.
/// </summary>
public record RegisterCommand : IRequest<Result<string>>
{
    /// <summary>
    /// User's email address (will be used for login).
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's password (must meet security requirements).
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match Password).
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Indicates whether to create a new workspace for this user.
    /// If true, workspace name is required.
    /// </summary>
    public bool CreateWorkspace { get; init; } = true;

    /// <summary>
    /// Name of the workspace to create (required if CreateWorkspace is true).
    /// </summary>
    public string? WorkspaceName { get; init; }
}

/// <summary>
/// Handler for the RegisterCommand.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _context;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IEmailService emailService,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _emailService = emailService;
        _context = context;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _identityService.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<string>.Failure(new[] { "A user with this email address already exists." });
        }

        // Create the user
        var createResult = await _identityService.CreateUserAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!createResult.Succeeded)
        {
            return Result<string>.Failure(createResult.Errors);
        }

        var userId = createResult.Data!;

        // Generate email verification token
        var verificationToken = await _identityService.GenerateEmailVerificationTokenAsync(userId);

        // Send verification email
        await _emailService.SendEmailVerificationAsync(
            request.Email,
            request.FirstName,
            verificationToken,
            cancellationToken);

        // Create workspace if requested
        if (request.CreateWorkspace && !string.IsNullOrWhiteSpace(request.WorkspaceName))
        {
            var organization = new IgnaCheck.Domain.Entities.Organization
            {
                Name = request.WorkspaceName,
                Slug = GenerateSlug(request.WorkspaceName),
                CreatedBy = userId,
                IsActive = true,
                SubscriptionTier = "Free",
                TrialEndsAt = DateTime.UtcNow.AddDays(14), // 14-day free trial
                MaxMembers = 5, // Free tier limit
                MaxProjects = 3,
                MaxStorageGb = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync(cancellationToken);

            // Add user as owner of the organization
            var organizationMember = new IgnaCheck.Domain.Entities.OrganizationMember
            {
                OrganizationId = organization.Id,
                UserId = userId,
                Role = IgnaCheck.Domain.Constants.WorkspaceRoles.Owner,
                JoinedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.OrganizationMembers.Add(organizationMember);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<string>.Success(userId);
    }

    private static string GenerateSlug(string name)
    {
        // Simple slug generation: lowercase, replace spaces with hyphens, remove special chars
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove special characters
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remove consecutive hyphens
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Append random suffix to ensure uniqueness
        slug += "-" + Guid.NewGuid().ToString("N")[..6];

        return slug;
    }
}
