using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.ApproveReview;

/// <summary>
/// Handles approval of product reviews.
/// </summary>
public sealed class ApproveReviewCommandHandler
    : IRequestHandler<ApproveReviewCommand, ApproveReviewResponse>
{
    private readonly IProductReviewRepository _reviewRepository;
    private readonly ILogger<ApproveReviewCommandHandler> _logger;

    public ApproveReviewCommandHandler(
        IProductReviewRepository reviewRepository,
        ILogger<ApproveReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApproveReviewResponse> Handle(
        ApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
        {
            _logger.LogWarning("Review not found: {ReviewId}", request.ReviewId);
            throw new NotFoundException("Review not found");
        }

        review.Approve();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review approved: {ReviewId}", request.ReviewId);

        return new ApproveReviewResponse(
            ReviewId: review.Id,
            Status: review.Status.ToString(),
            Message: "Review has been approved and is now visible to customers"
        );
    }
}
