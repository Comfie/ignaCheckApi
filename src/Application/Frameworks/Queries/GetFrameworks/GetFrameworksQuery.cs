using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Frameworks.Queries.GetFrameworks;

/// <summary>
/// Query to get all available compliance frameworks.
/// Returns system frameworks and organization-specific custom frameworks.
/// </summary>
public record GetFrameworksQuery : IRequest<Result<List<FrameworkDto>>>
{
    /// <summary>
    /// Filter by framework category (optional).
    /// </summary>
    public FrameworkCategory? Category { get; init; }

    /// <summary>
    /// Filter to show only active frameworks (default: true).
    /// </summary>
    public bool ActiveOnly { get; init; } = true;

    /// <summary>
    /// Include control count in response (default: true).
    /// </summary>
    public bool IncludeControlCount { get; init; } = true;
}

/// <summary>
/// Framework DTO for list display.
/// </summary>
public record FrameworkDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Version { get; init; } = string.Empty;
    public FrameworkCategory Category { get; init; }
    public string? IssuingAuthority { get; init; }
    public DateTime? PublicationDate { get; init; }
    public DateTime? EffectiveDate { get; init; }
    public bool IsSystemFramework { get; init; }
    public bool IsActive { get; init; }
    public int ControlCount { get; init; }
}

/// <summary>
/// Handler for GetFrameworksQuery.
/// </summary>
public class GetFrameworksQueryHandler : IRequestHandler<GetFrameworksQuery, Result<List<FrameworkDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetFrameworksQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<List<FrameworkDto>>> Handle(GetFrameworksQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _tenantService.GetCurrentTenantId();

        // Build query: system frameworks + org-specific custom frameworks
        var query = _context.ComplianceFrameworks
            .Where(f => f.IsSystemFramework || (organizationId != null && f.OrganizationId == organizationId.Value))
            .AsQueryable();

        // Apply filters
        if (request.Category.HasValue)
        {
            query = query.Where(f => f.Category == request.Category.Value);
        }

        if (request.ActiveOnly)
        {
            query = query.Where(f => f.IsActive);
        }

        // Execute query with projections
        var frameworks = await query
            .Select(f => new FrameworkDto
            {
                Id = f.Id,
                Code = f.Code,
                Name = f.Name,
                Description = f.Description,
                Version = f.Version,
                Category = f.Category,
                IssuingAuthority = f.IssuingAuthority,
                PublicationDate = f.PublicationDate,
                EffectiveDate = f.EffectiveDate,
                IsSystemFramework = f.IsSystemFramework,
                IsActive = f.IsActive,
                ControlCount = request.IncludeControlCount ? f.Controls.Count : 0
            })
            .OrderBy(f => f.Category)
            .ThenBy(f => f.Name)
            .ToListAsync(cancellationToken);

        return Result<List<FrameworkDto>>.Success(frameworks);
    }
}
