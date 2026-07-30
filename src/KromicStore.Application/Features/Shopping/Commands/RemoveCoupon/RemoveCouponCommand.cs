using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveCoupon;

/// <summary>
/// Command to remove an applied coupon from a checkout session.
/// </summary>
public sealed record RemoveCouponCommand(Guid CheckoutSessionId) : IRequest<RemoveCouponResponse>;

/// <summary>
/// Response for RemoveCoupon command.
/// </summary>
public sealed record RemoveCouponResponse(
    Guid CheckoutSessionId,
    bool CouponRemoved,
    decimal Total);
