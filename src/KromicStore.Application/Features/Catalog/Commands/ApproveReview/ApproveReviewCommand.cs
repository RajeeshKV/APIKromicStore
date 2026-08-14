using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.ApproveReview;

/// <summary>
/// Command to approve a product review for public display.
/// </summary>
public sealed record ApproveReviewCommand(
    Guid ReviewId
) : IRequest<ApproveReviewResponse>;

/// <summary>
/// Response from approve review operation.
/// </summary>
public sealed record ApproveReviewResponse(
    Guid ReviewId,
    string Status,
    string Message
);
