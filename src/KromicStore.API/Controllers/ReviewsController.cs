using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using MediatR;
using KromicStore.Application.Features.Catalog.Commands.ApproveReview;
using KromicStore.Application.Features.Catalog.Commands.RejectReview;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for product reviews and ratings.
/// Customers can submit, view, and manage product reviews.
/// </summary>
[ApiController]
[Route("api/v1/products/{productId:guid}/reviews")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IProductReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<ReviewsController> _logger;
    private readonly IMediator _mediator;

    public ReviewsController(
        IProductReviewRepository reviewRepository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService,
        IApplicationDbContext dbContext,
        ILogger<ReviewsController> logger,
        IMediator mediator)
    {
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all approved reviews for a product (customer-facing).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="skip">Number of reviews to skip (default: 0).</param>
    /// <param name="take">Number of reviews to return (default: 20, max: 100).</param>
    /// <response code="200">Returns approved reviews.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductReviewDto>>> GetApprovedReviews(
        Guid productId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productId == Guid.Empty)
                return BadRequest(new { message = "Invalid product ID" });

            // Verify product exists
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return NotFound(new { message = "Product not found" });
            }

            if (take > 100) take = 100;
            if (skip < 0) skip = 0;

            var reviews = await _reviewRepository.GetApprovedByProductIdAsync(productId, skip, take, cancellationToken);
            var reviewDtos = reviews.Select(MapToDto).ToList();

            _logger.LogInformation("Retrieved {Count} approved reviews for product {ProductId}", reviewDtos.Count, productId);
            return Ok(reviewDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets review statistics for a product (average rating, review count).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <response code="200">Returns review statistics.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("stats")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReviewStatsDto>> GetReviewStats(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productId == Guid.Empty)
                return BadRequest(new { message = "Invalid product ID" });

            // Verify product exists
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return NotFound(new { message = "Product not found" });
            }

            var reviews = await _reviewRepository.GetApprovedByProductIdAsync(productId, 0, 1000, cancellationToken);
            var reviewList = reviews.ToList();

            if (reviewList.Count == 0)
            {
                return Ok(new ReviewStatsDto(
                    TotalReviews: 0,
                    AverageRating: 0m,
                    RatingDistribution: new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } }));
            }

            var stats = new ReviewStatsDto(
                TotalReviews: reviewList.Count,
                AverageRating: (decimal)reviewList.Average(r => r.Rating),
                RatingDistribution: new Dictionary<int, int>
                {
                    { 1, reviewList.Count(r => r.Rating == 1) },
                    { 2, reviewList.Count(r => r.Rating == 2) },
                    { 3, reviewList.Count(r => r.Rating == 3) },
                    { 4, reviewList.Count(r => r.Rating == 4) },
                    { 5, reviewList.Count(r => r.Rating == 5) }
                });

            _logger.LogInformation("Review stats for product {ProductId}: Avg={Average}, Count={Count}", 
                productId, stats.AverageRating, stats.TotalReviews);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving review stats for product {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Submits a new product review (authenticated customers only).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="request">Review details.</param>
    /// <response code="201">Review created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="409">Customer already reviewed this product.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductReviewDto>> SubmitReview(
        Guid productId,
        [FromBody] SubmitReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productId == Guid.Empty)
                return BadRequest(new { message = "Invalid product ID" });

            if (request == null || request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5" });

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Review title is required" });

            // Verify product exists
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return NotFound(new { message = "Product not found" });
            }

            var customerId = _currentUserService.UserId;
            if (customerId == Guid.Empty)
                return Unauthorized(new { message = "Invalid user context" });

            // Check if customer already reviewed this product
            var existingReview = await _reviewRepository.GetByProductAndCustomerAsync(productId, customerId, cancellationToken);
            if (existingReview != null)
            {
                _logger.LogWarning("Customer {CustomerId} already reviewed product {ProductId}", customerId, productId);
                return Conflict(new { message = "You have already reviewed this product" });
            }

            // Create review
            var review = ProductReview.Create(
                productId,
                customerId,
                request.Rating,
                request.Title,
                request.Comment);

            _reviewRepository.Add(review);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            var reviewDto = MapToDto(review);

            _logger.LogInformation("Review created. ReviewId: {ReviewId}, ProductId: {ProductId}, CustomerId: {CustomerId}, Rating: {Rating}",
                review.Id, productId, customerId, request.Rating);

            return CreatedAtAction(nameof(GetReview), new { reviewId = review.Id }, reviewDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review for product {ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a specific review by ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reviewId">The review ID.</param>
    /// <response code="200">Returns review details.</response>
    /// <response code="404">Review or product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{reviewId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductReviewDto>> GetReview(
        Guid productId,
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productId == Guid.Empty || reviewId == Guid.Empty)
                return BadRequest(new { message = "Invalid product or review ID" });

            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
            if (review == null || review.ProductId != productId)
            {
                _logger.LogWarning("Review not found: {ReviewId}", reviewId);
                return NotFound(new { message = "Review not found" });
            }

            // Only show approved reviews to anonymous users
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                if (review.Status != ReviewStatus.Approved)
                    return NotFound(new { message = "Review not found" });
            }

            var reviewDto = MapToDto(review);
            return Ok(reviewDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving review {ReviewId}", reviewId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates an existing review (customer or admin).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reviewId">The review ID.</param>
    /// <param name="request">Updated review details.</param>
    /// <response code="200">Review updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden - not the review owner.</response>
    /// <response code="404">Review not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductReviewDto>> UpdateReview(
        Guid productId,
        Guid reviewId,
        [FromBody] UpdateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productId == Guid.Empty || reviewId == Guid.Empty)
                return BadRequest(new { message = "Invalid product or review ID" });

            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
            if (review == null || review.ProductId != productId)
            {
                _logger.LogWarning("Review not found: {ReviewId}", reviewId);
                return NotFound(new { message = "Review not found" });
            }

            var customerId = _currentUserService.UserId;
            if (customerId != review.CustomerId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized update attempt on review {ReviewId} by user {UserId}", reviewId, customerId);
                return Forbid();
            }

            // Validate request
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5" });

            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
                return BadRequest(new { message = "Title is required and must not exceed 200 characters" });

            if (request.Comment != null && request.Comment.Length > 5000)
                return BadRequest(new { message = "Comment must not exceed 5000 characters" });

            // Update the review with new values
            review.UpdateReview(request.Title, request.Comment, request.Rating);

            // Persist changes
            _reviewRepository.Update(review);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review updated successfully. ReviewId: {ReviewId}", reviewId);

            var reviewDto = MapToDto(review);
            return Ok(reviewDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a review (customer or admin).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reviewId">The review ID.</param>
    /// <response code="204">Review deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden - not the review owner.</response>
    /// <response code="404">Review not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteReview(
        Guid productId,
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productId == Guid.Empty || reviewId == Guid.Empty)
                return BadRequest(new { message = "Invalid product or review ID" });

            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
            if (review == null || review.ProductId != productId)
            {
                _logger.LogWarning("Review not found: {ReviewId}", reviewId);
                return NotFound(new { message = "Review not found" });
            }

            var customerId = _currentUserService.UserId;
            if (customerId != review.CustomerId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized delete attempt on review {ReviewId} by user {UserId}", reviewId, customerId);
                return Forbid();
            }

            review.Delete();
            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review deleted. ReviewId: {ReviewId}", reviewId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Marks a review as helpful (customer operation).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reviewId">The review ID.</param>
    /// <response code="200">Marked as helpful.</response>
    /// <response code="404">Review not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{reviewId:guid}/helpful")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsHelpful(
        Guid productId,
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
            if (review == null || review.ProductId != productId)
                return NotFound(new { message = "Review not found" });

            review.MarkAsHelpful();
            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review marked helpful. ReviewId: {ReviewId}, HelpfulCount: {HelpfulCount}", reviewId, review.HelpfulCount);
            return Ok(new { helpfulCount = review.HelpfulCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking review as helpful: {ReviewId}", reviewId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Marks a review as unhelpful (customer operation).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reviewId">The review ID.</param>
    /// <response code="200">Marked as unhelpful.</response>
    /// <response code="404">Review not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{reviewId:guid}/unhelpful")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsUnhelpful(
        Guid productId,
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
            if (review == null || review.ProductId != productId)
                return NotFound(new { message = "Review not found" });

            review.MarkAsUnhelpful();
            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review marked unhelpful. ReviewId: {ReviewId}, UnhelpfulCount: {UnhelpfulCount}", reviewId, review.UnhelpfulCount);
            return Ok(new { unhelpfulCount = review.UnhelpfulCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking review as unhelpful: {ReviewId}", reviewId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // ── Helper Methods ──────────────────────────────────────────────────────

    private static ProductReviewDto MapToDto(ProductReview review)
    {
        return new ProductReviewDto(
            ReviewId: review.Id,
            ProductId: review.ProductId,
            CustomerId: review.CustomerId,
            Rating: review.Rating,
            Title: review.Title,
            Comment: review.Comment,
            HelpfulCount: review.HelpfulCount,
            UnhelpfulCount: review.UnhelpfulCount,
            Status: review.Status.ToString(),
            SubmittedOn: review.SubmittedOnUtc);
    }

    /// <summary>
    /// Approves a review for public display (admin only).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reviewId">The review ID.</param>
    /// <response code="200">Review approved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Review not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{reviewId:guid}/approve")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(typeof(ApproveReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ApproveReview(
        Guid productId,
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        var command = new ApproveReviewCommand(reviewId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rejects a review and removes it from public display (admin only).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reviewId">The review ID.</param>
    /// <param name="request">Rejection details with optional reason.</param>
    /// <response code="200">Review rejected successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Review not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{reviewId:guid}/reject")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(typeof(RejectReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RejectReview(
        Guid productId,
        Guid reviewId,
        [FromBody] RejectReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RejectReviewCommand(reviewId, request?.RejectionReason);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

// DTOs - Review Moderation
public sealed record RejectReviewRequest(
    string? RejectionReason = null
);

public sealed record ApproveReviewResponse(
    Guid ReviewId,
    string Status,
    string Message
);

public sealed record RejectReviewResponse(
    Guid ReviewId,
    string Status,
    string? RejectionReason,
    string Message
);

// ── Request/Response DTOs ───────────────────────────────────────────────────

public record SubmitReviewRequest(
    int Rating,
    string Title,
    string? Comment);

public record UpdateReviewRequest(
    int Rating,
    string Title,
    string? Comment);

public record ProductReviewDto(
    Guid ReviewId,
    Guid ProductId,
    Guid CustomerId,
    int Rating,
    string Title,
    string? Comment,
    int HelpfulCount,
    int UnhelpfulCount,
    string Status,
    DateTime SubmittedOn);

public record ReviewStatsDto(
    int TotalReviews,
    decimal AverageRating,
    Dictionary<int, int> RatingDistribution);
