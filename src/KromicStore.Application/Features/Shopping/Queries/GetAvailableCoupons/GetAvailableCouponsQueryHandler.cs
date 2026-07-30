using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Queries.GetAvailableCoupons;

/// <summary>
/// Handler for GetAvailableCoupons query.
/// Returns available coupon codes for the customer.
/// In a production system, these would be retrieved from a database/repository.
/// </summary>
public sealed class GetAvailableCouponsQueryHandler : IRequestHandler<GetAvailableCouponsQuery, GetAvailableCouponsResponse>
{
    private readonly ILogger<GetAvailableCouponsQueryHandler> _logger;

    public GetAvailableCouponsQueryHandler(ILogger<GetAvailableCouponsQueryHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<GetAvailableCouponsResponse> Handle(GetAvailableCouponsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving available coupons. MinimumOrderAmount: {MinAmount}, MaxResults: {MaxResults}", 
            query.MinimumOrderAmount, query.MaxResults);

        // TODO: In production, fetch from Coupon repository with filters:
        // - ExpiresOnUtc > UtcNow (not expired)
        // - IsActive = true
        // - MinimumOrderAmount <= query.MinimumOrderAmount (if provided)
        // - Order by discount percentage descending
        // - Limit to MaxResults

        var availableCoupons = new List<CouponDto>
        {
            new CouponDto(
                CouponCode: "WELCOME10",
                Description: "Welcome discount - 10% off",
                DiscountPercentage: 10m,
                MaxDiscountAmount: 50m,
                MinimumOrderAmount: 0m,
                ExpiresOnUtc: DateTime.UtcNow.AddDays(30)),
            new CouponDto(
                CouponCode: "SUMMER20",
                Description: "Summer sale - 20% off orders over $50",
                DiscountPercentage: 20m,
                MaxDiscountAmount: 100m,
                MinimumOrderAmount: 50m,
                ExpiresOnUtc: DateTime.UtcNow.AddDays(60)),
            new CouponDto(
                CouponCode: "FREESHIP",
                Description: "Free shipping on orders over $100",
                DiscountPercentage: 0m,
                MaxDiscountAmount: null,
                MinimumOrderAmount: 100m,
                ExpiresOnUtc: DateTime.UtcNow.AddDays(15))
        };

        // Filter by minimum order amount if provided
        if (query.MinimumOrderAmount.HasValue)
        {
            availableCoupons = availableCoupons
                .Where(c => c.MinimumOrderAmount <= query.MinimumOrderAmount.Value)
                .ToList();
            _logger.LogInformation("Filtered coupons by minimum order amount: {Count} coupons available", availableCoupons.Count);
        }

        // Limit results
        var maxResults = query.MaxResults ?? 10;
        availableCoupons = availableCoupons.Take(maxResults).ToList();

        _logger.LogInformation("Retrieved {Count} available coupons", availableCoupons.Count);

        return Task.FromResult(new GetAvailableCouponsResponse(
            Coupons: availableCoupons,
            Count: availableCoupons.Count));
    }
}
