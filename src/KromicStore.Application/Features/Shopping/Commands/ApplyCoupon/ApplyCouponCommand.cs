using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.ApplyCoupon;

/// <summary>
/// Command to apply a coupon code to a checkout session.
/// </summary>
public sealed record ApplyCouponCommand(
    Guid CheckoutSessionId,
    string CouponCode) : IRequest<ApplyCouponResponse>;

/// <summary>
/// Response for ApplyCoupon command.
/// </summary>
public sealed record ApplyCouponResponse(
    Guid CheckoutSessionId,
    string CouponCode,
    decimal DiscountAmount,
    decimal Total);
