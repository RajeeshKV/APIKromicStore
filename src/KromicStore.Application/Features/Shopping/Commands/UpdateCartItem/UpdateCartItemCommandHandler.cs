using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateCartItem;

/// <summary>
/// Handler for UpdateCartItem command.
/// Updates the quantity of a specific cart item or removes it if quantity is 0.
/// </summary>
public sealed class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, UpdateCartItemResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateCartItemCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCartItemCommandHandler(
        ICartRepository cartRepository,
        IApplicationDbContext dbContext,
        ILogger<UpdateCartItemCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<UpdateCartItemResponse> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating cart item: CartId={CartId}, ProductId={ProductId}, NewQuantity={NewQuantity}",
            command.CartId, command.ProductId, command.NewQuantity);

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

        // Find the existing item
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == command.ProductId && i.ProductVariantId == command.VariantId);

        if (existingItem == null)
        {
            _logger.LogWarning(
                "Cart item not found: CartId={CartId}, ProductId={ProductId}",
                command.CartId, command.ProductId);
            throw new InvalidOperationException($"Item not found in cart");
        }

        var itemRemoved = false;
        decimal lineTotal = 0;
        int finalQuantity = existingItem.Quantity;
        decimal unitPrice = existingItem.UnitPrice;

        try
        {
            if (command.NewQuantity == 0)
            {
                // Remove the item
                cart.RemoveItem(command.ProductId, command.VariantId);
                itemRemoved = true;
                _logger.LogInformation("Item removed from cart: CartId={CartId}, ProductId={ProductId}", command.CartId, command.ProductId);
            }
            else
            {
                // Update quantity
                cart.UpdateItemQuantity(command.ProductId, command.NewQuantity, command.VariantId);
                finalQuantity = command.NewQuantity;
                lineTotal = command.NewQuantity * unitPrice;
                _logger.LogInformation(
                    "Item quantity updated: CartId={CartId}, ProductId={ProductId}, NewQuantity={NewQuantity}",
                    command.CartId, command.ProductId, command.NewQuantity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item: CartId={CartId}", command.CartId);
            throw;
        }

        _cartRepository.Update(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cart item updated successfully: CartId={CartId}, ProductId={ProductId}",
            command.CartId, command.ProductId);

        return new UpdateCartItemResponse(
            CartId: cart.Id,
            ProductId: command.ProductId,
            VariantId: command.VariantId,
            Quantity: finalQuantity,
            UnitPrice: unitPrice,
            LineTotal: lineTotal,
            CartItemsCount: cart.GetItemsCount(),
            CartSubTotal: cart.GetSubtotal(),
            ItemRemoved: itemRemoved);
    }
}
