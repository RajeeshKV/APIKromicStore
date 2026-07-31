using MediatR;

namespace KromicStore.Application.Features.Orders.Commands.InitializePayment;

/// <summary>
/// Initialize a payment for an order.
/// Creates a Payment aggregate in Pending status.
/// </summary>
public sealed class InitializePaymentCommand : IRequest<InitializePaymentResponse>
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid TenantId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Provider { get; set; }
}

public sealed class InitializePaymentResponse
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime InitiatedOnUtc { get; set; }
}
