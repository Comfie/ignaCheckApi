using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Notifications.Queries.GetNotifications;

/// <summary>
/// Query to get user's notifications with filtering.
/// </summary>
public record GetNotificationsQuery : IRequest<Result<List<NotificationDto>>>
{
    /// <summary>
    /// Filter by notification type (optional).
    /// </summary>
    public NotificationType? Type { get; init; }

    /// <summary>
    /// Filter by read status (optional - null returns all, true returns read, false returns unread).
    /// </summary>
    public bool? IsRead { get; init; }

    /// <summary>
    /// Maximum number of notifications to return (default: 50, max: 200).
    /// </summary>
    public int Limit { get; init; } = 50;

    /// <summary>
    /// Offset for pagination.
    /// </summary>
    public int Offset { get; init; } = 0;
}

/// <summary>
/// Notification DTO.
/// </summary>
public record NotificationDto
{
    public Guid Id { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Link { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
    public bool IsRead { get; init; }
    public DateTime? ReadDate { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handler for GetNotificationsQuery.
/// </summary>
public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetNotificationsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<NotificationDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Validate limit
        var limit = Math.Min(request.Limit, 200);

        // Base query - get user's notifications
        var query = _context.Notifications
            .Where(n => n.UserId == _currentUser.Id)
            .AsQueryable();

        // Apply filters
        if (request.Type.HasValue)
        {
            query = query.Where(n => n.Type == request.Type.Value);
        }

        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        // Get notifications with pagination
        var notifications = await query
            .OrderByDescending(n => n.Created)
            .Skip(request.Offset)
            .Take(limit)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                Link = n.Link,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                IsRead = n.IsRead,
                ReadDate = n.ReadDate,
                CreatedAt = n.Created
            })
            .ToListAsync(cancellationToken);

        return Result<List<NotificationDto>>.Success(notifications);
    }
}
