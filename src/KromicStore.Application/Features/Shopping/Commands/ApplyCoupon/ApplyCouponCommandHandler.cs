using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.ApplyCoupon;

/// <summary>
/// Handler for ApplyCoupon command.
/// Applies a coupon code to a checkout session and calculates discount.
/// </summary>
public sealed class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, ApplyCouponResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<ApplyCouponCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public ApplyCouponCommandHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<ApplyCouponCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<ApplyCouponResponse> Handle(ApplyCouponCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying coupon {CouponCode} to checkout session {CheckoutSessionId}", command.CouponCode, command.CheckoutSessionId);

        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");

        var checkoutSession = await _checkoutSessionRepository.GetByIdAsync(command.CheckoutSessionId, cancellationToken);
        if (checkoutSession == null)
        {
            _logger.LogWarning("Checkout session not found: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException($"Checkout session with ID {command.CheckoutSessionId} not found");
        }

        if (checkoutSession.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new UnauthorizedAccessException("Cannot access checkout session from another tenant");
        }

        // Verify checkout session is in draft state
        if (checkoutSession.Status.ToString() != "Draft")
        {
            _logger.LogWarning("Cannot apply coupon to checkout session not in Draft state: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Coupon can only be applied to checkout sessions in Draft state");
        }

        // TODO: In a real implementation, validate coupon against a Coupon repository
        // For now, we'll just apply a fixed discount calculation
        // This would typically check:
        // - Coupon exists and is active
        // - Coupon is valid for the tenant
        // - Coupon has not expired
        // - Minimum purchase amount is met
        // - Usage limits are not exceeded

        // Apply coupon (fixed 10% discount for demo)
        var discountAmount = checkoutSession.SubTotal * 0.10m;
        checkoutSession.ApplyCoupon(command.CouponCode, discountAmount);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Coupon applied to checkout session: {CheckoutSessionId}", command.CheckoutSessionId);

        return new ApplyCouponResponse(
            CheckoutSessionId: checkoutSession.Id,
            CouponCode: checkoutSession.CouponCode ?? "No coupon applied",
            DiscountAmount: checkoutSession.DiscountAmount,
            Total: checkoutSession.GrandTotal);
    }
}
