using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.AddToWishlist;

/// <summary>
/// Handler for AddToWishlist command.
/// Adds a product to a customer's wishlist.
/// </summary>
public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, AddToWishlistResponse>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<AddToWishlistCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public AddToWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        IApplicationDbContext dbContext,
        ILogger<AddToWishlistCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<AddToWishlistResponse> Handle(AddToWishlistCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding product {ProductId} to wishlist {WishlistId}", command.ProductId, command.WishlistId);

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

        // Check if product already exists in wishlist
        bool isNew = !wishlist.ContainsProduct(command.ProductId);

        if (isNew)
        {
            wishlist.AddItem(command.ProductId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product added to wishlist: {WishlistId}", command.WishlistId);
        }
        else
        {
            _logger.LogInformation("Product already in wishlist: {WishlistId}", command.WishlistId);
        }

        return new AddToWishlistResponse(
            WishlistId: wishlist.Id,
            ProductId: command.ProductId,
            ItemsCount: wishlist.GetItemsCount(),
            IsNew: isNew);
    }
}
