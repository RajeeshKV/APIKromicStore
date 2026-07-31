using MediatR;

namespace KromicStore.Application.CustomerPortal.Queries.GetNotificationHistory;

/// <summary>
/// Query to retrieve customer notification history with pagination.
/// </summary>
public sealed class GetNotificationHistoryQuery : IRequest<GetNotificationHistoryResponse>
{
    public Guid CustomerId { get; set; }
    public bool? IsRead { get; set; }
    public string? NotificationType { get; set; }
    public DateTime? FromDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class NotificationDto
{
    public Guid NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ReadOnUtc { get; set; }
}

public sealed class GetNotificationHistoryResponse
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
