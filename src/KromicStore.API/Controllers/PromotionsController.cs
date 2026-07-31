using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Promotions;
using CreateDiscountCommand = KromicStore.Application.Features.Promotions.Commands.CreateDiscount.CreateDiscountCommand;
using CreateDiscountResponse = KromicStore.Application.Features.Promotions.Commands.CreateDiscount.CreateDiscountResponse;
using ApplyCouponCommand = KromicStore.Application.Features.Promotions.Commands.ApplyCoupon.ApplyCouponCommand;
using ApplyCouponResponse = KromicStore.Application.Features.Promotions.Commands.ApplyCoupon.ApplyCouponResponse;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for discount and coupon management.
/// Tenants can create and manage discounts and coupon codes.
/// </summary>
[ApiController]
[Route("api/v1/promotions")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
public class PromotionsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotionsController"/> class.
    /// </summary>
    public PromotionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Creates a new discount for the tenant's store.
    /// </summary>
    /// <param name="request">Discount creation request.</param>
    /// <returns>Created discount details.</returns>
    /// <response code="201">Discount created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("discounts")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DiscountDto>> CreateDiscount(
        [FromBody] CreateDiscountRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDiscountCommand
        {
            Name = request.Name,
            Description = request.Name,
            Type = KromicStore.Domain.Promotions.Entities.DiscountType.PercentageAmount,
            PercentageAmount = request.DiscountPercentage / 100,
            ValidFromUtc = DateTime.UtcNow,
            ValidToUtc = DateTime.UtcNow.AddYears(1)
        };

        var response = await _mediator.Send(command, cancellationToken);

        var discountDto = new DiscountDto
        {
            DiscountId = response.DiscountId,
            Name = response.Name,
            DiscountPercentage = request.DiscountPercentage,
            MinOrderValue = request.MinOrderValue,
            IsActive = response.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetDiscount), new { discountId = response.DiscountId }, discountDto);
    }

    /// <summary>
    /// Gets a specific discount by ID.
    /// </summary>
    /// <param name="discountId">The discount ID.</param>
    /// <returns>Discount details.</returns>
    /// <response code="200">Returns discount details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Discount not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("discounts/{discountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DiscountDto>> GetDiscount(
        Guid discountId,
        CancellationToken cancellationToken = default)
    {
        // For now, return placeholder
        // In production, query handler would retrieve from DB
        return NotFound();
    }

    /// <summary>
    /// Updates an existing discount.
    /// </summary>
    /// <param name="discountId">The discount ID.</param>
    /// <param name="request">Discount update request.</param>
    /// <returns>Updated discount details.</returns>
    /// <response code="200">Discount updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Discount not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("discounts/{discountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DiscountDto>> UpdateDiscount(
        Guid discountId,
        [FromBody] UpdateDiscountRequest request,
        CancellationToken cancellationToken = default)
    {
        // Update handler would be sent here
        // For now, return placeholder
        return NotFound();
    }

    /// <summary>
    /// Deletes a discount.
    /// </summary>
    /// <param name="discountId">The discount ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Discount deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Discount not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("discounts/{discountId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDiscount(
        Guid discountId,
        CancellationToken cancellationToken = default)
    {
        // Delete handler would be sent here
        // For now, return success
        return NoContent();
    }

    /// <summary>
    /// Creates a new coupon code for the tenant's store.
    /// </summary>
    /// <param name="request">Coupon creation request.</param>
    /// <returns>Created coupon details.</returns>
    /// <response code="201">Coupon created successfully.</response>
    /// <response code="400">Validation error or duplicate code.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="409">Coupon code already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("coupons")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CouponDto>> CreateCoupon(
        [FromBody] CreateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        // Create coupon handler would be sent here
        // For now, return placeholder
        var couponDto = new CouponDto
        {
            CouponId = Guid.NewGuid(),
            Code = request.Code,
            DiscountPercentage = request.DiscountPercentage,
            MaxUsageCount = request.MaxUsageCount,
            TimesUsed = 0,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetCoupon), new { couponCode = request.Code }, couponDto);
    }

    /// <summary>
    /// Gets a specific coupon by code.
    /// </summary>
    /// <param name="couponCode">The coupon code.</param>
    /// <returns>Coupon details.</returns>
    /// <response code="200">Returns coupon details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Coupon not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("coupons/{couponCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CouponDto>> GetCoupon(
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        // Get coupon handler would be sent here
        // For now, return placeholder
        return NotFound();
    }

    /// <summary>
    /// Updates an existing coupon.
    /// </summary>
    /// <param name="couponCode">The coupon code.</param>
    /// <param name="request">Coupon update request.</param>
    /// <returns>Updated coupon details.</returns>
    /// <response code="200">Coupon updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Coupon not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("coupons/{couponCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CouponDto>> UpdateCoupon(
        string couponCode,
        [FromBody] UpdateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        // Update coupon handler would be sent here
        // For now, return placeholder
        return NotFound();
    }

    /// <summary>
    /// Deletes a coupon.
    /// </summary>
    /// <param name="couponCode">The coupon code.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Coupon deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Coupon not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("coupons/{couponCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCoupon(
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        // Delete coupon handler would be sent here
        return NoContent();
    }

    /// <summary>
    /// Applies a coupon code to a cart or order (customer operation).
    /// </summary>
    /// <param name="couponCode">The coupon code to apply.</param>
    /// <returns>Application result with discount amount.</returns>
    /// <response code="200">Coupon applied successfully.</response>
    /// <response code="400">Invalid coupon or validation error.</response>
    /// <response code="404">Coupon not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("coupons/{couponCode}/apply")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ApplyCoupon(
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        var command = new ApplyCouponCommand { CouponCode = couponCode };
        var response = await _mediator.Send(command, cancellationToken);

        if (!response.IsValid)
            return BadRequest(new { message = "Coupon code is invalid or expired." });

        return Ok(new { discountAmount = response.DiscountAmount });
    }
}
