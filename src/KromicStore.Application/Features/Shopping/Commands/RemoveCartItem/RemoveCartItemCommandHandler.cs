using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveCartItem;

/// <summary>
/// Handler for RemoveCartItem command.
/// Removes a specific item from the cart.
/// </summary>
public sealed class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, RemoveCartItemResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<RemoveCartItemCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCartItemCommandHandler(
        ICartRepository cartRepository,
        IApplicationDbContext dbContext,
        ILogger<RemoveCartItemCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<RemoveCartItemResponse> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Removing item from cart: CartId={CartId}, ProductId={ProductId}",
            command.CartId, command.ProductId);

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

        // Check if item exists
        var itemExists = cart.Items.Any(i =>
            i.ProductId == command.ProductId && i.ProductVariantId == command.VariantId);

        try
        {
            cart.RemoveItem(command.ProductId, command.VariantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cart: CartId={CartId}", command.CartId);
            throw;
        }

        _cartRepository.Update(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (itemExists)
        {
            _logger.LogInformation(
                "Item removed from cart successfully: CartId={CartId}, ProductId={ProductId}",
                command.CartId, command.ProductId);
        }

        return new RemoveCartItemResponse(
            CartId: cart.Id,
            ProductId: command.ProductId,
            VariantId: command.VariantId,
            ItemFound: itemExists,
            CartItemsCount: cart.GetItemsCount(),
            CartSubTotal: cart.GetSubtotal());
    }
}
