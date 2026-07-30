using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shopping.Entities;

/// <summary>
/// WishlistItem entity representing a single item in a wishlist.
/// Part of the Wishlist aggregate root.
/// </summary>
public sealed class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime AddedOnUtc { get; private set; }

    private WishlistItem()
    {
    }

    private WishlistItem(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Create a new wishlist item.
    /// </summary>
    public static WishlistItem Create(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        var item = new WishlistItem(Guid.NewGuid())
        {
            ProductId = productId,
            AddedOnUtc = DateTime.UtcNow
        };

        return item;
    }
}
