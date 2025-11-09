using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Notifications.Commands.MarkNotificationsAsRead;

/// <summary>
/// Command to mark notifications as read.
/// </summary>
public record MarkNotificationsAsReadCommand : IRequest<Result>
{
    /// <summary>
    /// List of notification IDs to mark as read.
    /// If empty, marks all unread notifications as read.
    /// </summary>
    public List<Guid> NotificationIds { get; init; } = new();
}

/// <summary>
/// Handler for MarkNotificationsAsReadCommand.
/// </summary>
public class MarkNotificationsAsReadCommandHandler : IRequestHandler<MarkNotificationsAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public MarkNotificationsAsReadCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MarkNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Get notifications to mark as read
        var query = _context.Notifications
            .Where(n => n.UserId == _currentUser.Id && !n.IsRead);

        // If specific IDs provided, filter by them
        if (request.NotificationIds.Any())
        {
            query = query.Where(n => request.NotificationIds.Contains(n.Id));
        }

        var notifications = await query.ToListAsync(cancellationToken);

        if (!notifications.Any())
        {
            return Result.Success(); // Nothing to update
        }

        // Mark as read
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
