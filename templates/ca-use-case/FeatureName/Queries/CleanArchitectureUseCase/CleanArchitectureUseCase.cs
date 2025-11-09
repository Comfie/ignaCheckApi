using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.FeatureName.Queries.IgnaCheckUseCase;

public record IgnaCheckUseCaseQuery : IRequest<object>
{
}

public class IgnaCheckUseCaseQueryValidator : AbstractValidator<IgnaCheckUseCaseQuery>
{
    public IgnaCheckUseCaseQueryValidator()
    {
    }
}

public class IgnaCheckUseCaseQueryHandler : IRequestHandler<IgnaCheckUseCaseQuery, object>
{
    private readonly IApplicationDbContext _context;

    public IgnaCheckUseCaseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(IgnaCheckUseCaseQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
