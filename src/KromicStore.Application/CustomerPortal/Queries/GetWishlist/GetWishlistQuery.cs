using MediatR;

namespace KromicStore.Application.CustomerPortal.Queries.GetWishlist;

/// <summary>
/// Query to retrieve customer wishlist with pagination.
/// </summary>
public sealed class GetWishlistQuery : IRequest<GetWishlistResponse>
{
    public Guid CustomerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public sealed class WishlistItemDto
{
    public Guid WishlistItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime AddedOnUtc { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class GetWishlistResponse
{
    public List<WishlistItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public decimal TotalValue { get; set; }
}
