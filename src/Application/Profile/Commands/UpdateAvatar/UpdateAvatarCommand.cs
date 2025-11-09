using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Profile.Commands.UpdateAvatar;

/// <summary>
/// Command to update the current user's avatar.
/// </summary>
public record UpdateAvatarCommand : IRequest<Result<UpdateAvatarResponse>>
{
    /// <summary>
    /// Avatar URL. For now, this accepts a URL.
    /// In production, this would be replaced with file upload functionality.
    /// </summary>
    public string AvatarUrl { get; init; } = string.Empty;
}

/// <summary>
/// Response for avatar update.
/// </summary>
public record UpdateAvatarResponse
{
    /// <summary>
    /// Updated avatar URL.
    /// </summary>
    public string AvatarUrl { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the UpdateAvatarCommand.
/// </summary>
public class UpdateAvatarCommandHandler : IRequestHandler<UpdateAvatarCommand, Result<UpdateAvatarResponse>>
{
    private readonly IUser _currentUser;
    private readonly IApplicationDbContext _context;

    public UpdateAvatarCommandHandler(
        IUser currentUser,
        IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<Result<UpdateAvatarResponse>> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<UpdateAvatarResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Get user from database (need EF tracking)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);
        if (user == null)
        {
            return Result<UpdateAvatarResponse>.Failure(new[] { "User not found." });
        }

        // Update avatar URL
        user.AvatarUrl = request.AvatarUrl;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UpdateAvatarResponse
        {
            AvatarUrl = user.AvatarUrl ?? string.Empty
        };

        return Result<UpdateAvatarResponse>.Success(response);
    }
}
