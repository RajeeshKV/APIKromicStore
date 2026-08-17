using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.API.Contracts.Promotions;
using CreateDiscountCommand = KromicStore.Application.Features.Promotions.Commands.CreateDiscount.CreateDiscountCommand;
using CreateDiscountResponse = KromicStore.Application.Features.Promotions.Commands.CreateDiscount.CreateDiscountResponse;
using ApplyCouponCommand = KromicStore.Application.Features.Promotions.Commands.ApplyCoupon.ApplyCouponCommand;
using ApplyCouponResponse = KromicStore.Application.Features.Promotions.Commands.ApplyCoupon.ApplyCouponResponse;
using CreateCampaignCommand = KromicStore.Application.Features.Promotions.Commands.CreateCampaign.CreateCampaignCommand;
using CreateCampaignResponse = KromicStore.Application.Features.Promotions.Commands.CreateCampaign.CreateCampaignResponse;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for promotions (discounts, coupons, campaigns).
/// Only TenantAdmin and StoreManager roles can access write operations.
/// Coupon apply endpoint is public (storefront use).
/// SuperAdmin gets 403 on all write/read operations.
/// Routes: /api/v1/tenant/promotions/*
/// </summary>
[Route("api/v1/tenant/promotions")]
public class PromotionsController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    public PromotionsController(IMediator mediator, ILogger<PromotionsController> logger) : base(logger)
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DiscountDto>> CreateDiscount(
        [FromBody] CreateDiscountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Discount name is required" });
            }

            var command = new CreateDiscountCommand
            {
                Name = request.Name,
                Description = request.Name,
                Type = KromicStore.Domain.Promotions.Entities.DiscountType.PercentageAmount,
                PercentageAmount = request.DiscountPercentage / 100m,
                ValidFromUtc = DateTime.UtcNow,
                ValidToUtc = DateTime.UtcNow.AddMonths(1)
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

            _logger.LogInformation("Discount created. DiscountId: {DiscountId}, Name: {Name}", response.DiscountId, response.Name);
            return CreatedAtAction(nameof(GetDiscount), new { discountId = response.DiscountId }, discountDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Discount creation validation failed");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discount");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<DiscountDto>> GetDiscount(
        Guid discountId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement GetDiscountQuery to retrieve from repository
        _logger.LogWarning("GetDiscount not fully implemented yet for {DiscountId}", discountId);
        return Task.FromResult<ActionResult<DiscountDto>>(NotFound());
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<DiscountDto>> UpdateDiscount(
        Guid discountId,
        [FromBody] UpdateDiscountRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement UpdateDiscountCommand
        _logger.LogWarning("UpdateDiscount not fully implemented yet");
        return Task.FromResult<ActionResult<DiscountDto>>(NotFound());
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteDiscount(
        Guid discountId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement DeleteDiscountCommand
        return Task.FromResult<IActionResult>(NoContent());
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CouponDto>> CreateCoupon(
        [FromBody] CreateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
            {
                return Task.FromResult<ActionResult<CouponDto>>(BadRequest(new { message = "Coupon code is required" }));
            }

            // TODO: Implement CreateCouponCommand to save to repository
            _logger.LogWarning("CreateCoupon not fully implemented yet");
            
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

            return Task.FromResult<ActionResult<CouponDto>>(CreatedAtAction(nameof(GetCoupon), new { couponCode = request.Code }, couponDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coupon");
            return Task.FromResult<ActionResult<CouponDto>>(StatusCode(StatusCodes.Status500InternalServerError));
        }
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CouponDto>> GetCoupon(
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement GetCouponQuery to retrieve from repository
        _logger.LogWarning("GetCoupon not fully implemented yet for {CouponCode}", couponCode);
        return Task.FromResult<ActionResult<CouponDto>>(NotFound());
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CouponDto>> UpdateCoupon(
        string couponCode,
        [FromBody] UpdateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement UpdateCouponCommand
        _logger.LogWarning("UpdateCoupon not fully implemented yet");
        return Task.FromResult<ActionResult<CouponDto>>(NotFound());
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
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteCoupon(
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement DeleteCouponCommand
        return Task.FromResult<IActionResult>(NoContent());
    }

    /// <summary>
    /// Applies a coupon code to a cart or order (tenant admin validation endpoint).
    /// For customer-facing coupon apply, use POST /api/v1/storefront/coupons/{couponCode}/apply
    /// </summary>
    /// <param name="couponCode">The coupon code to apply.</param>
    /// <returns>Application result with discount amount.</returns>
    /// <response code="200">Coupon applied successfully.</response>
    /// <response code="400">Invalid coupon or validation error.</response>
    /// <response code="404">Coupon not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("coupons/{couponCode}/apply")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ApplyCoupon(
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return BadRequest(new { message = "Coupon code is required" });
            }

            var command = new ApplyCouponCommand { CouponCode = couponCode };
            var response = await _mediator.Send(command, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogWarning("Coupon validation failed: {CouponCode}", couponCode);
                return BadRequest(new { message = "Coupon code is invalid or expired." });
            }

            _logger.LogInformation("Coupon applied successfully: {CouponCode}, Discount: {Amount}", couponCode, response.DiscountAmount);
            return Ok(new { message = "Coupon applied successfully", discountAmount = response.DiscountAmount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying coupon: {CouponCode}", couponCode);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Creates a new promotional campaign linking multiple discounts.
    /// </summary>
    /// <param name="request">Campaign creation request.</param>
    /// <returns>Created campaign details.</returns>
    /// <response code="201">Campaign created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("campaigns")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CampaignDto>> CreateCampaign(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Campaign name is required" });
            }

            if (request.StartDate >= request.EndDate)
            {
                return BadRequest(new { message = "End date must be after start date" });
            }

            var command = new CreateCampaignCommand
            {
                Name = request.Name,
                Description = request.Description,
                StartDateUtc = request.StartDate,
                EndDateUtc = request.EndDate,
                DiscountIds = request.DiscountIds ?? new()
            };

            var response = await _mediator.Send(command, cancellationToken);

            var campaignDto = new CampaignDto(
                CampaignId: response.CampaignId,
                Name: response.Name,
                DiscountCount: response.DiscountCount,
                IsActive: response.IsActive,
                StartDate: request.StartDate,
                EndDate: request.EndDate,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow
            );

            _logger.LogInformation("Campaign created. CampaignId: {CampaignId}, Name: {Name}, DiscountCount: {DiscountCount}", 
                response.CampaignId, response.Name, response.DiscountCount);

            return CreatedAtAction(nameof(GetCampaign), new { campaignId = response.CampaignId }, campaignDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Campaign creation validation failed");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a specific campaign by ID.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <returns>Campaign details.</returns>
    /// <response code="200">Returns campaign details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Campaign not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("campaigns/{campaignId}")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CampaignDto>> GetCampaign(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement GetCampaignQuery to retrieve from repository
        _logger.LogWarning("GetCampaign not fully implemented yet for {CampaignId}", campaignId);
        return Task.FromResult<ActionResult<CampaignDto>>(NotFound());
    }

    /// <summary>
    /// Gets all campaigns for the tenant's store (admin view).
    /// </summary>
    /// <returns>List of campaigns.</returns>
    /// <response code="200">Returns active campaigns.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("campaigns")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<CampaignDto>>> GetActiveCampaigns(CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement GetActiveCampaignsQuery to retrieve from repository
            _logger.LogWarning("GetActiveCampaigns not fully implemented yet");
            return Task.FromResult<ActionResult<IEnumerable<CampaignDto>>>(Ok(Enumerable.Empty<CampaignDto>()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active campaigns");
            return Task.FromResult<ActionResult<IEnumerable<CampaignDto>>>(
                StatusCode(StatusCodes.Status500InternalServerError));
        }
    }
}

// ── Request/Response DTOs ───────────────────────────────────────────────────

public record CreateCampaignRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    List<Guid>? DiscountIds);

public record CampaignDto(
    Guid CampaignId,
    string Name,
    int DiscountCount,
    bool IsActive,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

