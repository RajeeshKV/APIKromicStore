using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Queries.GetCart;

/// <summary>
/// Handler for GetCart query.
/// Retrieves a shopping cart with all items.
/// </summary>
public sealed class GetCartQueryHandler : IRequestHandler<GetCartQuery, GetCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<GetCartQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        ILogger<GetCartQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<GetCartResponse> Handle(GetCartQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving cart: {CartId}", query.CartId);

        var cart = await _cartRepository.GetByIdAsync(query.CartId, cancellationToken);
        if (cart == null)
        {
            _logger.LogWarning("Cart not found: {CartId}", query.CartId);
            throw new InvalidOperationException($"Cart with ID {query.CartId} not found");
        }

        // Verify cart belongs to the tenant
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");
        if (cart.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to cart: {CartId}", query.CartId);
            throw new UnauthorizedAccessException("Cannot access cart from another tenant");
        }

        // Map items
        var items = cart.Items.Select(i => new CartItemDto(
            ProductId: i.ProductId,
            VariantId: i.ProductVariantId,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            LineTotal: i.LineTotal)).ToList();

        return new GetCartResponse(
            CartId: cart.Id,
            CustomerId: cart.CustomerId,
            AnonymousSessionId: cart.AnonymousSessionId,
            Currency: cart.Currency,
            Items: items,
            ItemsCount: cart.GetItemsCount(),
            SubTotal: cart.GetSubtotal(),
            LastActivityOnUtc: cart.LastActivityOnUtc,
            ExpiresOnUtc: cart.ExpiresOnUtc,
            IsExpired: cart.IsExpired);
    }
}
