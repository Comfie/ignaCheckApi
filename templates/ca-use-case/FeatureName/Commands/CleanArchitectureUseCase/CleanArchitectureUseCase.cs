using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.FeatureName.Commands.IgnaCheckUseCase;

public record IgnaCheckUseCaseCommand : IRequest<object>
{
}

public class IgnaCheckUseCaseCommandValidator : AbstractValidator<IgnaCheckUseCaseCommand>
{
    public IgnaCheckUseCaseCommandValidator()
    {
    }
}

public class IgnaCheckUseCaseCommandHandler : IRequestHandler<IgnaCheckUseCaseCommand, object>
{
    private readonly IApplicationDbContext _context;

    public IgnaCheckUseCaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(IgnaCheckUseCaseCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
