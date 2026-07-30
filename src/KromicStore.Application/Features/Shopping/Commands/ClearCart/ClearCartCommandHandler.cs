using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.ClearCart;

/// <summary>
/// Handler for ClearCart command.
/// Removes all items from a shopping cart.
/// </summary>
public sealed class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, ClearCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<ClearCartCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public ClearCartCommandHandler(
        ICartRepository cartRepository,
        IApplicationDbContext dbContext,
        ILogger<ClearCartCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<ClearCartResponse> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clearing cart: {CartId}", command.CartId);

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

        // Store state before clearing
        var previousItemsCount = cart.GetItemsCount();
        var previousSubTotal = cart.GetSubtotal();

        try
        {
            cart.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart: {CartId}", command.CartId);
            throw;
        }

        _cartRepository.Update(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cart cleared successfully: {CartId}, PreviousItemsCount={PreviousItemsCount}",
            command.CartId, previousItemsCount);

        return new ClearCartResponse(
            CartId: cart.Id,
            PreviousItemsCount: previousItemsCount,
            PreviousSubTotal: previousSubTotal,
            CartNowEmpty: cart.IsEmpty);
    }
}
