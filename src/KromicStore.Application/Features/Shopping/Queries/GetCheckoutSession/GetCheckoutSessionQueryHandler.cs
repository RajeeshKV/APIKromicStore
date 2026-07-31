using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Features.Shopping.Dtos;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Queries.GetCheckoutSession;

/// <summary>
/// Handler for GetCheckoutSession query.
/// Retrieves a checkout session with all items and details.
/// </summary>
public sealed class GetCheckoutSessionQueryHandler : IRequestHandler<GetCheckoutSessionQuery, GetCheckoutSessionResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly ILogger<GetCheckoutSessionQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetCheckoutSessionQueryHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        ILogger<GetCheckoutSessionQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<GetCheckoutSessionResponse> Handle(GetCheckoutSessionQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving checkout session: {CheckoutSessionId}", query.CheckoutSessionId);

        var checkoutSession = await _checkoutSessionRepository.GetByIdAsync(query.CheckoutSessionId, cancellationToken);
        if (checkoutSession == null)
        {
            _logger.LogWarning("Checkout session not found: {CheckoutSessionId}", query.CheckoutSessionId);
            throw new InvalidOperationException($"Checkout session with ID {query.CheckoutSessionId} not found");
        }

        // Verify checkout session belongs to the tenant
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");
        if (checkoutSession.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to checkout session: {CheckoutSessionId}", query.CheckoutSessionId);
            throw new UnauthorizedAccessException("Cannot access checkout session from another tenant");
        }

        // Map items
        var items = checkoutSession.Items.Select(i => new CheckoutItemDto(
            ProductId: i.ProductId,
            VariantId: i.ProductVariantId,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            LineTotal: i.LineTotal)).ToList();

        return new GetCheckoutSessionResponse(
            CheckoutSessionId: checkoutSession.Id,
            TenantId: checkoutSession.TenantId,
            CustomerId: checkoutSession.CustomerId,
            Currency: "USD",
            Items: items,
            ItemsCount: checkoutSession.Items.Count,
            SubTotal: checkoutSession.SubTotal,
            BillingAddress: null,
            ShippingAddress: null,
            ShippingMethodId: checkoutSession.ShippingMethod,
            ShippingCost: checkoutSession.ShippingAmount,
            CouponCode: checkoutSession.CouponCode,
            DiscountAmount: checkoutSession.DiscountAmount,
            Total: checkoutSession.GrandTotal,
            PaymentMethod: checkoutSession.PaymentMethod ?? "",
            PaymentStatus: checkoutSession.Status.ToString(),
            Status: checkoutSession.Status.ToString(),
            CreatedOnUtc: checkoutSession.CreatedOnUtc,
            LastModifiedOnUtc: checkoutSession.ModifiedOnUtc ?? checkoutSession.CreatedOnUtc);
    }
}
