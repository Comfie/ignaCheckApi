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
    private readonly IIdentityService _identityService;

    public UpdateAvatarCommandHandler(
        IUser currentUser,
        IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result<UpdateAvatarResponse>> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<UpdateAvatarResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Update avatar URL using identity service
        var updateResult = await _identityService.UpdateUserAvatarAsync(_currentUser.Id, request.AvatarUrl);
        if (!updateResult)
        {
            return Result<UpdateAvatarResponse>.Failure(new[] { "Failed to update avatar." });
        }

        var response = new UpdateAvatarResponse
        {
            AvatarUrl = request.AvatarUrl
        };

        return Result<UpdateAvatarResponse>.Success(response);
    }
}
