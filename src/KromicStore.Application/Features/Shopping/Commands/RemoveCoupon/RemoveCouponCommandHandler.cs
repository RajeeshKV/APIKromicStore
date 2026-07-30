using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveCoupon;

/// <summary>
/// Handler for RemoveCoupon command.
/// Removes an applied coupon from a checkout session.
/// </summary>
public sealed class RemoveCouponCommandHandler : IRequestHandler<RemoveCouponCommand, RemoveCouponResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<RemoveCouponCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public RemoveCouponCommandHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<RemoveCouponCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<RemoveCouponResponse> Handle(RemoveCouponCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing coupon from checkout session {CheckoutSessionId}", command.CheckoutSessionId);

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
            _logger.LogWarning("Cannot remove coupon from checkout session not in Draft state: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Coupon can only be removed from checkout sessions in Draft state");
        }

        // Check if coupon is applied
        bool hasCoupon = !string.IsNullOrWhiteSpace(checkoutSession.CouponCode);

        if (hasCoupon)
        {
            checkoutSession.RemoveCoupon();
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Coupon removed from checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
        }
        else
        {
            _logger.LogInformation("No coupon applied to checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
        }

        return new RemoveCouponResponse(
            CheckoutSessionId: checkoutSession.Id,
            CouponRemoved: hasCoupon,
            Total: checkoutSession.GrandTotal);
    }
}
