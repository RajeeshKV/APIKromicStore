using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Queries.GetWishlist;

/// <summary>
/// Handler for GetWishlist query.
/// Retrieves a wishlist with all items.
/// </summary>
public sealed class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, GetWishlistResponse>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ILogger<GetWishlistQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetWishlistQueryHandler(
        IWishlistRepository wishlistRepository,
        ILogger<GetWishlistQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<GetWishlistResponse> Handle(GetWishlistQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving wishlist: {WishlistId}", query.WishlistId);

        var wishlist = await _wishlistRepository.GetByIdAsync(query.WishlistId, cancellationToken);
        if (wishlist == null)
        {
            _logger.LogWarning("Wishlist not found: {WishlistId}", query.WishlistId);
            throw new InvalidOperationException($"Wishlist with ID {query.WishlistId} not found");
        }

        // Verify wishlist belongs to the tenant
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");
        if (wishlist.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to wishlist: {WishlistId}", query.WishlistId);
            throw new UnauthorizedAccessException("Cannot access wishlist from another tenant");
        }

        // Map items
        var items = wishlist.Items.Select(i => new WishlistItemDto(
            ProductId: i.ProductId,
            VariantId: null,
            AddedOnUtc: i.AddedOnUtc)).ToList();

        return new GetWishlistResponse(
            WishlistId: wishlist.Id,
            CustomerId: wishlist.CustomerId,
            Items: items,
            ItemsCount: wishlist.GetItemsCount(),
            CreatedOnUtc: wishlist.CreatedOnUtc,
            LastModifiedOnUtc: wishlist.ModifiedOnUtc ?? wishlist.CreatedOnUtc);
    }
}
