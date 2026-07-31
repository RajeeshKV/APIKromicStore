using KromicStore.Domain.Common;

namespace KromicStore.Domain.Orders.Entities;

/// <summary>
/// OrderTimeline value object representing an event in the order's lifecycle.
/// Provides an audit trail of all significant order events.
/// </summary>
public sealed class OrderTimeline : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string EventDescription { get; private set; } = string.Empty;
    public string Actor { get; private set; } = string.Empty;
    public DateTime CreatedOnUtc { get; private set; }
    
    private OrderTimeline()
    {
    }
    
    private OrderTimeline(Guid id) : base(id)
    {
    }
    
    /// <summary>
    /// Create a new timeline entry.
    /// </summary>
    public static OrderTimeline Create(Guid orderId, string eventDescription, string actor)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        if (string.IsNullOrWhiteSpace(eventDescription))
            throw new ArgumentException("EventDescription cannot be empty", nameof(eventDescription));
        
        if (string.IsNullOrWhiteSpace(actor))
            throw new ArgumentException("Actor cannot be empty", nameof(actor));
        
        return new OrderTimeline(Guid.NewGuid())
        {
            OrderId = orderId,
            EventDescription = eventDescription.Trim(),
            Actor = actor.Trim(),
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}

/// <summary>
/// OrderNote value object representing a note/comment on an order.
/// Used by support staff and customers to communicate about order issues.
/// </summary>
public sealed class OrderNote : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public DateTime CreatedOnUtc { get; private set; }
    public bool IsPublic { get; private set; } // True if visible to customer
    
    private OrderNote()
    {
    }
    
    private OrderNote(Guid id) : base(id)
    {
    }
    
    /// <summary>
    /// Create a new order note.
    /// </summary>
    public static OrderNote Create(Guid orderId, string content, string author, bool isPublic = false)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));
        
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty", nameof(author));
        
        return new OrderNote(Guid.NewGuid())
        {
            OrderId = orderId,
            Content = content.Trim(),
            Author = author.Trim(),
            IsPublic = isPublic,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}
