using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.AddToCart;

/// <summary>
/// Handler for AddToCart command.
/// Adds or updates cart items with quantity management.
/// </summary>
public sealed class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, AddToCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<AddToCartCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public AddToCartCommandHandler(
        ICartRepository cartRepository,
        IApplicationDbContext dbContext,
        ILogger<AddToCartCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<AddToCartResponse> Handle(AddToCartCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding item to cart: CartId={CartId}, ProductId={ProductId}, Quantity={Quantity}",
            command.CartId, command.ProductId, command.Quantity);

        // Get the cart
        var cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
        if (cart == null)
        {
            _logger.LogWarning("Cart not found: {CartId}", command.CartId);
            throw new InvalidOperationException($"Cart with ID {command.CartId} not found");
        }

        // Verify cart belongs to the tenant
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");
        if (cart.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to cart: {CartId}", command.CartId);
            throw new UnauthorizedAccessException("Cannot access cart from another tenant");
        }

        // Add or update item
        try
        {
            cart.AddItem(command.ProductId, command.UnitPrice, command.Quantity, command.VariantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart: {CartId}", command.CartId);
            throw;
        }

        _cartRepository.Update(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Item added to cart successfully: CartId={CartId}, ProductId={ProductId}",
            command.CartId, command.ProductId);

        // Find the item that was added/updated
        var cartItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == command.ProductId && i.ProductVariantId == command.VariantId);

        if (cartItem == null)
        {
            throw new InvalidOperationException("Failed to add item to cart");
        }

        return new AddToCartResponse(
            CartId: cart.Id,
            ProductId: command.ProductId,
            VariantId: command.VariantId,
            Quantity: cartItem.Quantity,
            UnitPrice: cartItem.UnitPrice,
            LineTotal: cartItem.LineTotal,
            CartItemsCount: cart.GetItemsCount(),
            CartSubTotal: cart.GetSubtotal());
    }
}
