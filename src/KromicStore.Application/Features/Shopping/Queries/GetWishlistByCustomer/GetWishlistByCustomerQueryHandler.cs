using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Queries.GetWishlistByCustomer;

/// <summary>
/// Handler for GetWishlistByCustomer query.
/// Retrieves the wishlist for a specific customer.
/// </summary>
public sealed class GetWishlistByCustomerQueryHandler : IRequestHandler<GetWishlistByCustomerQuery, GetWishlistByCustomerResponse>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ILogger<GetWishlistByCustomerQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetWishlistByCustomerQueryHandler(
        IWishlistRepository wishlistRepository,
        ILogger<GetWishlistByCustomerQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<GetWishlistByCustomerResponse> Handle(GetWishlistByCustomerQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving wishlist for customer: {CustomerId}", query.CustomerId);

        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
        if (wishlist == null)
        {
            _logger.LogInformation("No wishlist found for customer: {CustomerId}", query.CustomerId);
            throw new InvalidOperationException($"No wishlist found for customer {query.CustomerId}");
        }

        // Verify wishlist belongs to the tenant
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");
        if (wishlist.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to wishlist for customer: {CustomerId}", query.CustomerId);
            throw new UnauthorizedAccessException("Cannot access wishlist from another tenant");
        }

        // Map items
        var items = wishlist.Items.Select(i => new WishlistItemDto(
            ProductId: i.ProductId,
            VariantId: null,
            AddedOnUtc: i.AddedOnUtc)).ToList();

        return new GetWishlistByCustomerResponse(
            WishlistId: wishlist.Id,
            CustomerId: wishlist.CustomerId,
            Items: items,
            ItemsCount: wishlist.GetItemsCount(),
            CreatedOnUtc: wishlist.CreatedAtUtc,
            LastModifiedOnUtc: wishlist.ModifiedAtUtc);
    }
}
