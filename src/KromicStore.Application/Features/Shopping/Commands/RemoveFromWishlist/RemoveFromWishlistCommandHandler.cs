using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveFromWishlist;

/// <summary>
/// Handler for RemoveFromWishlist command.
/// Removes a product from a customer's wishlist.
/// </summary>
public sealed class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, RemoveFromWishlistResponse>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<RemoveFromWishlistCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public RemoveFromWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        IApplicationDbContext dbContext,
        ILogger<RemoveFromWishlistCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<RemoveFromWishlistResponse> Handle(RemoveFromWishlistCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing product {ProductId} from wishlist {WishlistId}", command.ProductId, command.WishlistId);

        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");

        var wishlist = await _wishlistRepository.GetByIdAsync(command.WishlistId, cancellationToken);
        if (wishlist == null)
        {
            _logger.LogWarning("Wishlist not found: {WishlistId}", command.WishlistId);
            throw new InvalidOperationException($"Wishlist with ID {command.WishlistId} not found");
        }

        // Verify wishlist belongs to the tenant
        if (wishlist.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to wishlist: {WishlistId}", command.WishlistId);
            throw new UnauthorizedAccessException("Cannot access wishlist from another tenant");
        }

        // Check if product exists in wishlist
        bool wasRemoved = wishlist.ContainsProduct(command.ProductId);

        if (wasRemoved)
        {
            wishlist.RemoveItem(command.ProductId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product removed from wishlist: {WishlistId}", command.WishlistId);
        }
        else
        {
            _logger.LogInformation("Product not found in wishlist: {WishlistId}", command.WishlistId);
        }

        return new RemoveFromWishlistResponse(
            WishlistId: wishlist.Id,
            ProductId: command.ProductId,
            ItemsCount: wishlist.GetItemsCount(),
            WasRemoved: wasRemoved);
    }
}
