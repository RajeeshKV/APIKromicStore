using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.RejectReview;

/// <summary>
/// Command to reject a product review.
/// </summary>
public sealed record RejectReviewCommand(
    Guid ReviewId,
    string? RejectionReason = null
) : IRequest<RejectReviewResponse>;

/// <summary>
/// Response from reject review operation.
/// </summary>
public sealed record RejectReviewResponse(
    Guid ReviewId,
    string Status,
    string? RejectionReason,
    string Message
);
