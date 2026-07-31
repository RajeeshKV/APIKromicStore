using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Queries.GetCartByCustomer;

/// <summary>
/// Handler for GetCartByCustomer query.
/// Retrieves the active shopping cart for a specific customer.
/// </summary>
public sealed class GetCartByCustomerQueryHandler : IRequestHandler<GetCartByCustomerQuery, GetCartByCustomerResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<GetCartByCustomerQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetCartByCustomerQueryHandler(
        ICartRepository cartRepository,
        ILogger<GetCartByCustomerQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<GetCartByCustomerResponse> Handle(GetCartByCustomerQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving cart for customer: {CustomerId}", query.CustomerId);

        var cart = await _cartRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
        if (cart == null)
        {
            _logger.LogInformation("No active cart found for customer: {CustomerId}", query.CustomerId);
            throw new InvalidOperationException($"No active cart found for customer {query.CustomerId}");
        }

        // Verify cart belongs to the tenant
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");
        if (cart.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to cart for customer: {CustomerId}", query.CustomerId);
            throw new UnauthorizedAccessException("Cannot access cart from another tenant");
        }

        // Map items
        var items = cart.Items.Select(i => new CartItemDto(
            ProductId: i.ProductId,
            VariantId: i.ProductVariantId,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            LineTotal: i.LineTotal)).ToList();

        return new GetCartByCustomerResponse(
            CartId: cart.Id,
            CustomerId: cart.CustomerId ?? query.CustomerId,
            Currency: cart.Currency,
            Items: items,
            ItemsCount: cart.GetItemsCount(),
            SubTotal: cart.GetSubtotal(),
            LastActivityOnUtc: cart.LastActivityOnUtc,
            ExpiresOnUtc: cart.ExpiresOnUtc,
            IsExpired: cart.IsExpired);
    }
}
