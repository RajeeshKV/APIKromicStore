using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.InitializePayment;

/// <summary>
/// Command to initialize payment for a checkout session.
/// </summary>
public sealed record InitializePaymentCommand(
    Guid CheckoutSessionId,
    string PaymentMethod) : IRequest<InitializePaymentResponse>;

/// <summary>
/// Response for InitializePayment command.
/// </summary>
public sealed record InitializePaymentResponse(
    Guid CheckoutSessionId,
    string PaymentMethod,
    decimal Amount,
    string PaymentStatus,
    string PaymentToken);
