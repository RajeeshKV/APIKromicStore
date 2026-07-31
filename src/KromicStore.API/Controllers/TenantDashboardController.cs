using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
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
using KromicStore.Application.Features.Tenants.Commands.UpdateStoreSettings;
using KromicStore.Application.Features.Tenants.Commands.UpdatePaymentSettings;

namespace KromicStore.API.Controllers;

/// <summary>
/// Tenant Dashboard API endpoints.
/// Provides access to store settings, analytics, and operational data.
/// </summary>
[ApiController]
[Route("api/v1/tenant/dashboard")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
public class TenantDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantDashboardController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets tenant dashboard overview with key metrics.
    /// </summary>
    /// <returns>Dashboard overview with metrics</returns>
    /// <response code="200">Returns dashboard overview.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview(CancellationToken cancellationToken = default)
    {
        // Get tenant ID from claims
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetDashboardOverviewQuery { TenantId = tenantId };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(new DashboardOverviewDto(
            TotalOrders: result.TotalOrders,
            TotalRevenue: result.TotalRevenue,
            ActiveCustomers: result.ActiveCustomers,
            LowStockProducts: result.LowStockProducts,
            PendingOrders: result.PendingOrders,
            TodaysSales: result.TodaysSales));
    }

    /// <summary>
    /// Gets store profile/settings.
    /// </summary>
    /// <returns>Store settings</returns>
    /// <response code="200">Returns store settings.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("store-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreSettingsDto>> GetStoreSettings(CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetStoreSettingsQuery { TenantId = tenantId };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(new StoreSettingsDto(
            result.TenantId,
            result.StoreName,
            result.Description,
            result.Email,
            result.PhoneNumber,
            result.CurrencyCode));
    }

    /// <summary>
    /// Updates store profile/settings.
    /// </summary>
    /// <param name="request">Store settings update request.</param>
    /// <returns>Updated store settings</returns>
    /// <response code="200">Settings updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("store-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreSettingsDto>> UpdateStoreSettings(
        [FromBody] UpdateStoreSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var command = new UpdateStoreSettingsCommand
        {
            TenantId = tenantId,
            StoreName = request.StoreName,
            Description = request.Description,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CurrencyCode = request.CurrencyCode
        };
        
        var result = await _mediator.Send(command, cancellationToken);
        
        // Return updated settings
        var getQuery = new GetStoreSettingsQuery { TenantId = tenantId };
        var settings = await _mediator.Send(getQuery, cancellationToken);
        
        return Ok(new StoreSettingsDto(
            settings.TenantId,
            settings.StoreName,
            settings.Description,
            settings.Email,
            settings.PhoneNumber,
            settings.CurrencyCode));
    }

    /// <summary>
    /// Gets analytics for the store (revenue, orders, customers, etc.).
    /// </summary>
    /// <param name="startDate">Start date for analytics (default: 30 days ago).</param>
    /// <param name="endDate">End date for analytics (default: today).</param>
    /// <returns>Analytics data</returns>
    /// <response code="200">Returns analytics.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoreAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var query = new GetStoreAnalyticsQuery
        {
            TenantId = tenantId,
            StartDate = startDate.Value,
            EndDate = endDate.Value
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(new StoreAnalyticsDto(
            startDate.Value,
            endDate.Value,
            result.TotalRevenue,
            result.OrderCount,
            result.CustomerCount,
            result.AverageOrderValue,
            result.ConversionRate));
    }

    /// <summary>
    /// Gets recent orders for the store.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 10, max: 50).</param>
    /// <param name="status">Optional: Filter by order status.</param>
    /// <returns>Recent orders</returns>
    /// <response code="200">Returns orders.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("recent-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetRecentOrders(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetStoreOrdersQuery
        {
            TenantId = tenantId,
            Skip = skip,
            Take = Math.Min(take, 50),
            Status = status
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        
        var dtos = result.Orders.Select(o => new OrderSummaryDto(
            o.Id,
            o.OrderNumber,
            o.CreatedOnUtc,
            o.Total,
            o.Status,
            1)).ToList();
        
        return Ok(dtos);
    }

    /// <summary>
    /// Gets low stock products for the store.
    /// </summary>
    /// <param name="threshold">Quantity threshold (default: 10).</param>
    /// <returns>Low stock products</returns>
    /// <response code="200">Returns low stock products.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("low-stock-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<LowStockProductDto>>> GetLowStockProducts(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetLowStockProductsQuery
        {
            TenantId = tenantId,
            ThresholdQty = threshold
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        
        var dtos = result.Products.Select(p => new LowStockProductDto(
            p.Id,
            p.Name,
            p.CurrentStock,
            p.ThresholdQty)).ToList();
        
        return Ok(dtos);
    }

    /// <summary>
    /// Gets top selling products for the store.
    /// </summary>
    /// <param name="take">Number of products to return (default: 10, max: 50).</param>
    /// <param name="days">Number of days to analyze (default: 30).</param>
    /// <returns>Top selling products</returns>
    /// <response code="200">Returns top products.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("top-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts(
        [FromQuery] int take = 10,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetTopProductsQuery
        {
            TenantId = tenantId,
            Limit = Math.Min(take, 50)
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        
        var dtos = result.Products.Select(p => new TopProductDto(
            p.Id,
            p.Name,
            p.SalesCount,
            p.Revenue)).ToList();
        
        return Ok(dtos);
    }

    /// <summary>
    /// Gets customer list with purchase history.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <param name="search">Optional: Search by name or email.</param>
    /// <returns>Customer list</returns>
    /// <response code="200">Returns customers.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("customers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> GetCustomers(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetStoreCustomersQuery
        {
            TenantId = tenantId,
            Skip = skip,
            Take = Math.Min(take, 100)
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        
        var dtos = result.Customers
            .Where(c => string.IsNullOrEmpty(search) || 
                       c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                       c.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(c => new CustomerSummaryDto(
                c.Id,
                c.Name,
                c.Email,
                c.TotalOrders,
                c.TotalSpent,
                c.LastOrderDate))
            .ToList();
        
        return Ok(dtos);
    }

    /// <summary>
    /// Gets store publish status.
    /// </summary>
    /// <returns>Publish status</returns>
    /// <response code="200">Returns publish status.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("publish-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PublishStatusDto>> GetPublishStatus(CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetPublishStatusQuery { TenantId = tenantId };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(new PublishStatusDto(
            result.IsPublished,
            result.PublishedOnUtc,
            result.StoreUrl,
            result.IsPublished));
    }

    /// <summary>
    /// Gets payment configuration.
    /// </summary>
    /// <returns>Payment settings</returns>
    /// <response code="200">Returns payment settings.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("payment-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentSettingsDto>> GetPaymentSettings(CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var query = new GetPaymentSettingsQuery { TenantId = tenantId };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(new PaymentSettingsDto(
            "Razorpay",
            result.RazorpayEnabled,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Updates payment configuration.
    /// </summary>
    /// <param name="request">Payment settings update request.</param>
    /// <returns>Updated payment settings</returns>
    /// <response code="200">Settings updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("payment-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentSettingsDto>> UpdatePaymentSettings(
        [FromBody] UpdatePaymentSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var command = new UpdatePaymentSettingsCommand
        {
            TenantId = tenantId,
            RazorpayEnabled = true,
            RazorpayKeyId = request.ApiKey,
            RazorpayKeySecret = request.ApiSecret
        };
        
        var result = await _mediator.Send(command, cancellationToken);
        
        // Return updated settings
        var getQuery = new GetPaymentSettingsQuery { TenantId = tenantId };
        var settings = await _mediator.Send(getQuery, cancellationToken);
        
        return Ok(new PaymentSettingsDto(
            "Razorpay",
            settings.RazorpayEnabled,
            DateTime.UtcNow));
    }
}

// DTOs
public record DashboardOverviewDto(
    int TotalOrders,
    decimal TotalRevenue,
    int ActiveCustomers,
    int LowStockProducts,
    int PendingOrders,
    decimal TodaysSales);

public record StoreSettingsDto(
    Guid TenantId,
    string StoreName,
    string? Description,
    string? Email,
    string? PhoneNumber,
    string? CurrencyCode);

public record UpdateStoreSettingsRequest(
    string StoreName,
    string? Description,
    string? Email,
    string? PhoneNumber,
    string? CurrencyCode);

public record StoreAnalyticsDto(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal TotalRevenue,
    int TotalOrders,
    int TotalCustomers,
    decimal AverageOrderValue,
    decimal ConversionRate);

public record LowStockProductDto(
    Guid ProductId,
    string ProductName,
    int CurrentStock,
    int ReorderLevel);

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    int OrderCount,
    decimal TotalRevenue);

public record CustomerSummaryDto(
    Guid CustomerId,
    string Name,
    string Email,
    int OrderCount,
    decimal TotalSpent,
    DateTime LastOrderDate);

public record PublishStatusDto(
    bool IsPublished,
    DateTime? PublishedDateUtc,
    string? PreviewUrl,
    bool ValidationPassed);

public record PaymentSettingsDto(
    string PaymentGateway,
    bool IsConfigured,
    DateTime? ConfiguredDateUtc);

public record UpdatePaymentSettingsRequest(
    string ApiKey,
    string ApiSecret);
