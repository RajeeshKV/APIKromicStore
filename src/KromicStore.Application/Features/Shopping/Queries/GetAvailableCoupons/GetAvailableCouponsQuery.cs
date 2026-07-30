using MediatR;

namespace KromicStore.Application.Features.Shopping.Queries.GetAvailableCoupons;

/// <summary>
/// Query to retrieve available coupon codes.
/// In a real implementation, this would filter by active, non-expired coupons.
/// </summary>
public sealed record GetAvailableCouponsQuery(
    decimal? MinimumOrderAmount = null,
    int? MaxResults = 10) : IRequest<GetAvailableCouponsResponse>;

/// <summary>
/// DTO for an available coupon.
/// </summary>
public sealed record CouponDto(
    string CouponCode,
    string Description,
    decimal DiscountPercentage,
    decimal? MaxDiscountAmount,
    decimal MinimumOrderAmount,
    DateTime? ExpiresOnUtc);

/// <summary>
/// Response for GetAvailableCoupons query.
/// </summary>
public sealed record GetAvailableCouponsResponse(
    List<CouponDto> Coupons,
    int Count);
