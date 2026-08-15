using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Customers;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.Application.Features.Customers.Queries.ExportCustomers;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for customer management.
/// Only TenantAdmin and StoreManager roles can access.
/// SuperAdmin gets 403.
/// Routes: /api/v1/tenant/customers/*
/// </summary>
[Route("api/v1/tenant/customers")]
public class CustomerManagementController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    public CustomerManagementController(IMediator mediator, ILogger<CustomerManagementController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all customers for the tenant's store with pagination.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Paginated list of customers.</returns>
    /// <response code="200">Returns list of customers.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        // Get customers from GetStoreCustomersQuery
        // For now, return empty list (handler would query DB)
        return Task.FromResult<ActionResult<IEnumerable<CustomerDto>>>(Ok(Enumerable.Empty<CustomerDto>()));
    }

    /// <summary>
    /// Searches for customers by name or email.
    /// </summary>
    /// <param name="searchTerm">Search term (name, email, or phone).</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Filtered list of customers.</returns>
    /// <response code="200">Returns matching customers.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers(
        [FromQuery] string searchTerm,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Task.FromResult<ActionResult<IEnumerable<CustomerDto>>>(BadRequest(new { message = "Search term cannot be empty." }));

        // Search handler would query DB with searchTerm
        return Task.FromResult<ActionResult<IEnumerable<CustomerDto>>>(Ok(Enumerable.Empty<CustomerDto>()));
    }

    /// <summary>
    /// Gets detailed information about a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>Customer details with preferences.</returns>
    /// <response code="200">Returns customer details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CustomerDto>> GetCustomer(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        // Get customer handler would retrieve from DB
        return Task.FromResult<ActionResult<CustomerDto>>(NotFound());
    }

    /// <summary>
    /// Gets order history for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>List of customer's orders.</returns>
    /// <response code="200">Returns customer orders.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{customerId}/orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<CustomerOrderDto>>> GetCustomerOrders(
        Guid customerId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        // Get customer orders handler would retrieve from DB
        return Task.FromResult<ActionResult<IEnumerable<CustomerOrderDto>>>(Ok(Enumerable.Empty<CustomerOrderDto>()));
    }

    /// <summary>
    /// Gets customer communication preferences.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>Customer preferences.</returns>
    /// <response code="200">Returns preferences.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{customerId}/preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CustomerPreferencesDto>> GetCustomerPreferences(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        // Get preferences handler would retrieve from DB
        return Task.FromResult<ActionResult<CustomerPreferencesDto>>(NotFound());
    }

    /// <summary>
    /// Updates customer communication preferences.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="request">Preferences update request.</param>
    /// <returns>Updated preferences.</returns>
    /// <response code="200">Preferences updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{customerId}/preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<CustomerPreferencesDto>> UpdateCustomerPreferences(
        Guid customerId,
        [FromBody] UpdateCustomerPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        // Update preferences handler would be sent here
        return Task.FromResult<ActionResult<CustomerPreferencesDto>>(NotFound());
    }

    /// <summary>
    /// Gets customer spending statistics.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>Customer spending data.</returns>
    /// <response code="200">Returns spending statistics.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{customerId}/statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult> GetCustomerStatistics(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        // Get statistics handler would retrieve from DB
        return Task.FromResult<ActionResult>(Ok(new
        {
            totalOrders = 0,
            totalSpent = 0m,
            averageOrderValue = 0m,
            lastOrderDate = (DateTime?)null,
            preferredCategory = (string?)null
        }));
    }

    /// <summary>
    /// Gets top customers by spending.
    /// </summary>
    /// <param name="limit">Number of customers to return (default: 10, max: 100).</param>
    /// <returns>Top customers by spending.</returns>
    /// <response code="200">Returns top customers.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("top-by-spending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<CustomerDto>>> GetTopCustomersBySpending(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1 || limit > 100)
            limit = 10;

        // Get top customers handler would retrieve from DB
        return Task.FromResult<ActionResult<IEnumerable<CustomerDto>>>(Ok(Enumerable.Empty<CustomerDto>()));
    }

    /// <summary>
    /// Exports all customers to CSV file.
    /// Tenant admins can export customer data for CRM integration and analysis.
    /// </summary>
    /// <returns>CSV file with customer data.</returns>
    /// <response code="200">Returns CSV file.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportCustomers(
        CancellationToken cancellationToken = default)
    {
        var query = new ExportCustomersQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return File(result.CsvData, "text/csv", result.FileName);
    }
}

