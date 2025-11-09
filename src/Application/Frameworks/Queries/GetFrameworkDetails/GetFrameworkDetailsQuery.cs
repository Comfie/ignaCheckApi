using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Frameworks.Queries.GetFrameworkDetails;

/// <summary>
/// Query to get detailed information about a specific compliance framework, including all its controls.
/// </summary>
public record GetFrameworkDetailsQuery : IRequest<Result<FrameworkDetailsDto>>
{
    public Guid FrameworkId { get; init; }

    /// <summary>
    /// Include full control details (default: true).
    /// </summary>
    public bool IncludeControls { get; init; } = true;
}

/// <summary>
/// Detailed framework DTO with controls.
/// </summary>
public record FrameworkDetailsDto
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
    public int TotalControls { get; init; }
    public int MandatoryControls { get; init; }
    public List<ControlDto> Controls { get; init; } = new();
}

/// <summary>
/// Control DTO for framework details.
/// </summary>
public record ControlDto
{
    public Guid Id { get; init; }
    public string ControlCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImplementationGuidance { get; init; }
    public string? Category { get; init; }
    public string? SubCategory { get; init; }
    public RiskLevel DefaultRiskLevel { get; init; }
    public int SortOrder { get; init; }
    public bool IsMandatory { get; init; }
}

/// <summary>
/// Handler for GetFrameworkDetailsQuery.
/// </summary>
public class GetFrameworkDetailsQueryHandler : IRequestHandler<GetFrameworkDetailsQuery, Result<FrameworkDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetFrameworkDetailsQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<FrameworkDetailsDto>> Handle(GetFrameworkDetailsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _tenantService.GetCurrentTenantId();

        // Query framework - must be either a system framework or belong to current organization
        var framework = await _context.ComplianceFrameworks
            .Where(f => f.Id == request.FrameworkId &&
                       (f.IsSystemFramework || (organizationId != null && f.OrganizationId == organizationId.Value)))
            .Select(f => new FrameworkDetailsDto
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
                TotalControls = f.Controls.Count,
                MandatoryControls = f.Controls.Count(c => c.IsMandatory),
                Controls = request.IncludeControls
                    ? f.Controls.OrderBy(c => c.SortOrder).Select(c => new ControlDto
                    {
                        Id = c.Id,
                        ControlCode = c.ControlCode,
                        Title = c.Title,
                        Description = c.Description,
                        ImplementationGuidance = c.ImplementationGuidance,
                        Category = c.Category,
                        SubCategory = c.SubCategory,
                        DefaultRiskLevel = c.DefaultRiskLevel,
                        SortOrder = c.SortOrder,
                        IsMandatory = c.IsMandatory
                    }).ToList()
                    : new List<ControlDto>()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (framework == null)
        {
            return Result<FrameworkDetailsDto>.Failure(new[] { "Framework not found or access denied." });
        }

        return Result<FrameworkDetailsDto>.Success(framework);
    }
}
