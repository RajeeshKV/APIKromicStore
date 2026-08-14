using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.RejectReview;

/// <summary>
/// Handles rejection of product reviews.
/// </summary>
public sealed class RejectReviewCommandHandler
    : IRequestHandler<RejectReviewCommand, RejectReviewResponse>
{
    private readonly IProductReviewRepository _reviewRepository;
    private readonly ILogger<RejectReviewCommandHandler> _logger;

    public RejectReviewCommandHandler(
        IProductReviewRepository reviewRepository,
        ILogger<RejectReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RejectReviewResponse> Handle(
        RejectReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
        {
            _logger.LogWarning("Review not found: {ReviewId}", request.ReviewId);
            throw new NotFoundException("Review not found");
        }

        review.Reject(request.RejectionReason);
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review rejected: {ReviewId}, Reason: {Reason}", 
            request.ReviewId, request.RejectionReason ?? "No reason provided");

        return new RejectReviewResponse(
            ReviewId: review.Id,
            Status: review.Status.ToString(),
            RejectionReason: request.RejectionReason,
            Message: "Review has been rejected and removed from public display"
        );
    }
}
