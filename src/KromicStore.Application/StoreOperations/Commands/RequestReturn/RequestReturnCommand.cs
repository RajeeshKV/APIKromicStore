using MediatR;

namespace KromicStore.Application.StoreOperations.Commands.RequestReturn;

/// <summary>
/// Command to create a return request.
/// </summary>
public sealed class RequestReturnCommand : IRequest<RequestReturnResponse>
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public int ItemCount { get; set; }
    public decimal ReturnAmount { get; set; }
}

public sealed class RequestReturnResponse
{
    public Guid ReturnRequestId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal ReturnAmount { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
