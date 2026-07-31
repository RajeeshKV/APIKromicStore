using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// Represents a customer review for a product.
/// Reviews include a rating and optional text feedback.
/// </summary>
public sealed class ProductReview : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Rating { get; private set; } // 1-5 stars
    public string Title { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public int HelpfulCount { get; private set; }
    public int UnhelpfulCount { get; private set; }
    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending;
    public DateTime SubmittedOnUtc { get; private set; }

    private ProductReview()
    {
    }

    private ProductReview(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Creates a new product review.
    /// </summary>
    public static ProductReview Create(
        Guid productId,
        Guid customerId,
        int rating,
        string title,
        string? comment = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Review title cannot be empty", nameof(title));

        var review = new ProductReview(Guid.NewGuid())
        {
            ProductId = productId,
            CustomerId = customerId,
            Rating = rating,
            Title = title.Trim(),
            Comment = comment?.Trim(),
            Status = ReviewStatus.Pending,
            SubmittedOnUtc = DateTime.UtcNow
        };

        return review;
    }

    /// <summary>
    /// Approves the review for public display.
    /// </summary>
    public void Approve()
    {
        if (Status != ReviewStatus.Pending && Status != ReviewStatus.Rejected)
            throw new InvalidOperationException($"Cannot approve review in {Status} status");

        Status = ReviewStatus.Approved;
    }

    /// <summary>
    /// Rejects the review and hides it from public display.
    /// </summary>
    public void Reject(string? reason = null)
    {
        if (Status == ReviewStatus.Deleted)
            throw new InvalidOperationException("Cannot reject a deleted review");

        Status = ReviewStatus.Rejected;
    }

    /// <summary>
    /// Soft deletes the review.
    /// </summary>
    public void Delete()
    {
        if (Status == ReviewStatus.Deleted)
            throw new InvalidOperationException("Review is already deleted");

        SoftDelete(DateTime.UtcNow, "Review deleted by customer or moderator");
        Status = ReviewStatus.Deleted;
    }

    /// <summary>
    /// Marks a review as helpful.
    /// </summary>
    public void MarkAsHelpful()
    {
        HelpfulCount++;
    }

    /// <summary>
    /// Marks a review as unhelpful.
    /// </summary>
    public void MarkAsUnhelpful()
    {
        UnhelpfulCount++;
    }

    /// <summary>
    /// Updates the review content (only for draft/pending reviews).
    /// </summary>
    public void UpdateReview(string title, string? comment, int rating)
    {
        if (Status == ReviewStatus.Approved)
            throw new InvalidOperationException("Cannot update an approved review");

        if (Status == ReviewStatus.Deleted)
            throw new InvalidOperationException("Cannot update a deleted review");

        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Review title cannot be empty", nameof(title));

        Rating = rating;
        Title = title.Trim();
        Comment = comment?.Trim();
    }
}

/// <summary>
/// Possible review statuses.
/// </summary>
public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Deleted = 3
}
