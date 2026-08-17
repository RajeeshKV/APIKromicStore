using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.Application.Features.Tenants.Queries.GetDashboardOverview;
using KromicStore.Application.Features.Tenants.Queries.GetStoreSettings;
using KromicStore.Application.Features.Tenants.Queries.GetStoreAnalytics;
using KromicStore.Application.Features.Tenants.Queries.GetStoreOrders;
using KromicStore.Application.Features.Orders.Queries.GetOrders;
using KromicStore.Application.Features.Tenants.Queries.GetLowStockProducts;
using KromicStore.Application.Features.Tenants.Queries.GetTopProducts;
using KromicStore.Application.Features.Tenants.Queries.GetStoreCustomers;
using KromicStore.Application.Features.Tenants.Queries.GetPublishStatus;
using KromicStore.Application.Features.Tenants.Queries.GetPaymentSettings;
using KromicStore.Application.Features.Tenants.Queries.GetTenant;
using KromicStore.Application.Features.Tenants.Commands.UpdateStoreSettings;
using KromicStore.Application.Features.Tenants.Commands.UpdatePaymentSettings;
using KromicStore.Application.Features.Tenants.Commands.AddCustomDomain;
using KromicStore.Application.Features.Tenants.Commands.RemoveCustomDomain;
using KromicStore.Application.Features.Tenants.Commands.VerifyCustomDomain;
using KromicStore.Application.Features.Tenants.Commands.UpdateSubdomain;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for dashboard and store management.
/// Only TenantAdmin and StoreManager roles can access.
/// SuperAdmin gets 403.
/// Routes: /api/v1/tenant/dashboard/*
/// </summary>
[Route("api/v1/tenant/dashboard")]
public class TenantDashboardController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    public TenantDashboardController(IMediator mediator, ILogger<TenantDashboardController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads tenantId from the JWT "tenantId" claim.
    /// Returns Guid.Empty if missing or unparseable.
    /// </summary>
    private Guid GetTenantIdFromClaims()
    {
        var claim = User.FindFirst("tenantId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // ── Endpoints ─────────────────────────────────────────────────────────────

    /// <summary>Gets tenant dashboard overview with key metrics.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetDashboardOverviewQuery { TenantId = tenantId }, cancellationToken);

        return Ok(new DashboardOverviewDto(
            result.TotalOrders,
            result.TotalRevenue,
            result.ActiveCustomers,
            result.LowStockProducts,
            result.PendingOrders,
            result.TodaysSales));
    }

    /// <summary>Gets store profile/settings.</summary>
    [HttpGet("store-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreSettingsDto>> GetStoreSettings(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetStoreSettingsQuery { TenantId = tenantId }, cancellationToken);

        return Ok(new StoreSettingsDto(result.TenantId, result.StoreName, result.Description, result.Email, result.PhoneNumber, result.CurrencyCode));
    }

    /// <summary>Updates store profile/settings.</summary>
    [HttpPut("store-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreSettingsDto>> UpdateStoreSettings(
        [FromBody] UpdateStoreSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        await _mediator.Send(new UpdateStoreSettingsCommand
        {
            TenantId    = tenantId,
            StoreName   = request.StoreName,
            Description = request.Description,
            Email       = request.Email,
            PhoneNumber = request.PhoneNumber,
            CurrencyCode = request.CurrencyCode
        }, cancellationToken);

        var settings = await _mediator.Send(new GetStoreSettingsQuery { TenantId = tenantId }, cancellationToken);
        return Ok(new StoreSettingsDto(settings.TenantId, settings.StoreName, settings.Description, settings.Email, settings.PhoneNumber, settings.CurrencyCode));
    }

    /// <summary>Gets store analytics.</summary>
    [HttpGet("analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate   = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end   = endDate   ?? DateTime.UtcNow;

        var result = await _mediator.Send(new GetStoreAnalyticsQuery { TenantId = tenantId, StartDate = start, EndDate = end }, cancellationToken);

        return Ok(new StoreAnalyticsDto(start, end, result.TotalRevenue, result.OrderCount, result.CustomerCount, result.AverageOrderValue, result.ConversionRate));
    }

    /// <summary>Gets recent orders.</summary>
    [HttpGet("recent-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetRecentOrders(
        [FromQuery] int    skip   = 0,
        [FromQuery] int    take   = 10,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetStoreOrdersQuery
        {
            TenantId = tenantId,
            Skip     = skip,
            Take     = Math.Min(take, 50),
            Status   = status
        }, cancellationToken);

        var dtos = result.Orders.Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.CreatedOnUtc, o.Total, o.Status, 1));
        return Ok(dtos);
    }

    /// <summary>Gets low stock products.</summary>
    [HttpGet("low-stock-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<LowStockProductDto>>> GetLowStockProducts(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetLowStockProductsQuery { TenantId = tenantId, ThresholdQty = threshold }, cancellationToken);
        var dtos = result.Products.Select(p => new LowStockProductDto(p.Id, p.Name, p.CurrentStock, p.ThresholdQty));
        return Ok(dtos);
    }

    /// <summary>Gets top selling products.</summary>
    [HttpGet("top-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts(
        [FromQuery] int take = 10,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetTopProductsQuery { TenantId = tenantId, Limit = Math.Min(take, 50) }, cancellationToken);
        var dtos = result.Products.Select(p => new TopProductDto(p.Id, p.Name, p.SalesCount, p.Revenue));
        return Ok(dtos);
    }

    /// <summary>Gets customer list.</summary>
    [HttpGet("customers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> GetCustomers(
        [FromQuery] int    skip   = 0,
        [FromQuery] int    take   = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetStoreCustomersQuery { TenantId = tenantId, Skip = skip, Take = Math.Min(take, 100) }, cancellationToken);

        var dtos = result.Customers
            .Where(c => string.IsNullOrEmpty(search) ||
                        c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(c => new CustomerSummaryDto(c.Id, c.Name, c.Email, c.TotalOrders, c.TotalSpent, c.LastOrderDate));

        return Ok(dtos);
    }

    /// <summary>Gets store publish status.</summary>
    [HttpGet("publish-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PublishStatusDto>> GetPublishStatus(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetPublishStatusQuery { TenantId = tenantId }, cancellationToken);
        return Ok(new PublishStatusDto(result.IsPublished, result.PublishedOnUtc, result.StoreUrl, result.IsPublished));
    }

    /// <summary>Gets payment configuration.</summary>
    [HttpGet("payment-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentSettingsDto>> GetPaymentSettings(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetPaymentSettingsQuery { TenantId = tenantId }, cancellationToken);
        return Ok(new PaymentSettingsDto("Razorpay", result.RazorpayEnabled, DateTime.UtcNow));
    }

    /// <summary>Updates payment configuration.</summary>
    [HttpPut("payment-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentSettingsDto>> UpdatePaymentSettings(
        [FromBody] UpdatePaymentSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        await _mediator.Send(new UpdatePaymentSettingsCommand
        {
            TenantId         = tenantId,
            RazorpayEnabled  = true,
            RazorpayKeyId    = request.ApiKey,
            RazorpayKeySecret = request.ApiSecret
        }, cancellationToken);

        var settings = await _mediator.Send(new GetPaymentSettingsQuery { TenantId = tenantId }, cancellationToken);
        return Ok(new PaymentSettingsDto("Razorpay", settings.RazorpayEnabled, DateTime.UtcNow));
    }

    // ── Domain Management ─────────────────────────────────────────────────────

    /// <summary>Gets all domains for this tenant.</summary>
    [HttpGet("domains")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TenantDomainDto>>> GetDomains(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetTenantQuery(tenantId), cancellationToken);
        return Ok(result.Domains);
    }

    /// <summary>Adds a custom domain.</summary>
    [HttpPost("domains")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AddCustomDomainResponse>> AddDomain(
        [FromBody] AddDomainRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new AddCustomDomainCommand(tenantId, request.CustomDomain, request.SetPrimary), cancellationToken);
        return CreatedAtAction(nameof(GetDomains), result);
    }

    /// <summary>Removes a custom domain.</summary>
    [HttpDelete("domains")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveDomain(
        [FromBody] RemoveDomainRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        await _mediator.Send(new RemoveCustomDomainCommand(tenantId, request.CustomDomain), cancellationToken);
        return NoContent();
    }

    /// <summary>Triggers DNS verification for a custom domain.</summary>
    [HttpPost("domains/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<VerifyCustomDomainResponse>> VerifyDomain(
        [FromBody] VerifyDomainRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new VerifyCustomDomainCommand(tenantId, request.CustomDomain), cancellationToken);
        return Ok(result);
    }

    // ── Subdomain ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the store's platform subdomain (e.g. mystore → newname).
    /// The new subdomain must be available. After saving, the store URL changes to
    /// https://newname.kromic.in — inform the user they must use the new URL.
    /// </summary>
    /// <response code="200">Subdomain updated. Returns new store URL.</response>
    /// <response code="400">Validation error (format, reserved, etc.).</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="409">Subdomain already taken.</response>
    [HttpPatch("subdomain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateSubdomainResponse>> UpdateSubdomain(
        [FromBody] UpdateSubdomainRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantIdFromClaims();
        if (tenantId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(
            new UpdateSubdomainCommand(tenantId, request.NewSubdomain),
            cancellationToken);

        return Ok(result);
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record DashboardOverviewDto(int TotalOrders, decimal TotalRevenue, int ActiveCustomers, int LowStockProducts, int PendingOrders, decimal TodaysSales);
public record StoreSettingsDto(Guid TenantId, string StoreName, string? Description, string? Email, string? PhoneNumber, string? CurrencyCode);
public record UpdateStoreSettingsRequest(string StoreName, string? Description, string? Email, string? PhoneNumber, string? CurrencyCode);
public record StoreAnalyticsDto(DateTime PeriodStart, DateTime PeriodEnd, decimal TotalRevenue, int TotalOrders, int TotalCustomers, decimal AverageOrderValue, decimal ConversionRate);
public record LowStockProductDto(Guid ProductId, string ProductName, int CurrentStock, int ReorderLevel);
public record TopProductDto(Guid ProductId, string ProductName, int OrderCount, decimal TotalRevenue);
public record CustomerSummaryDto(Guid CustomerId, string Name, string Email, int OrderCount, decimal TotalSpent, DateTime LastOrderDate);
public record PublishStatusDto(bool IsPublished, DateTime? PublishedDateUtc, string? PreviewUrl, bool ValidationPassed);
public record PaymentSettingsDto(string PaymentGateway, bool IsConfigured, DateTime? ConfiguredDateUtc);
public record UpdatePaymentSettingsRequest(string ApiKey, string ApiSecret);
public record AddDomainRequest(string CustomDomain, bool SetPrimary = false);
public record RemoveDomainRequest(string CustomDomain);
public record VerifyDomainRequest(string CustomDomain);

/// <summary>Request body for PATCH /api/v1/tenant/dashboard/subdomain</summary>
public record UpdateSubdomainRequest(string NewSubdomain);

