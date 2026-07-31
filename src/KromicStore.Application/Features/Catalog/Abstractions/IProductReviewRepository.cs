using KromicStore.Domain.Catalog.Entities;

namespace KromicStore.Application.Features.Catalog.Abstractions;

/// <summary>
/// Repository abstraction for product reviews.
/// </summary>
public interface IProductReviewRepository
{
    Task<ProductReview?> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetByProductIdAsync(Guid productId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetApprovedByProductIdAsync(Guid productId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetByCustomerIdAsync(Guid customerId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetPendingAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<ProductReview?> GetByProductAndCustomerAsync(Guid productId, Guid customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<int> GetReviewCountAsync(Guid productId, CancellationToken cancellationToken = default);
    void Add(ProductReview review);
    void Update(ProductReview review);
    void Remove(ProductReview review);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
