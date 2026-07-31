using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Tenants.Queries.GetContactRequests;
using KromicStore.Application.Features.Tenants.Commands.CreateContactRequest;
using KromicStore.Application.Features.Tenants.Commands.ResolveContactRequest;
using KromicStore.Domain.Tenants;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for contact request management.
/// Allows creating and managing customer contact requests.
/// </summary>
[ApiController]
[Route("api/v1/contact-requests")]
public class ContactRequestController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactRequestController"/> class.
    /// </summary>
    public ContactRequestController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets contact requests for the platform.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20).</param>
    /// <param name="status">Filter by status (optional).</param>
    /// <param name="searchTerm">Search term to filter requests (optional).</param>
    /// <returns>List of contact requests.</returns>
    /// <response code="200">Returns list of contact requests.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [Authorize(Roles = "SuperUser,TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetContactRequestsResponse>> GetContactRequests(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] ContactRequestStatus? status = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetContactRequestsQuery
        {
            Skip = skip,
            Take = take,
            StatusFilter = status,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new contact request.
    /// </summary>
    /// <param name="command">Contact request creation command.</param>
    /// <returns>Created contact request.</returns>
    /// <response code="201">Contact request created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateContactRequestResponse>> CreateContactRequest(
        [FromBody] CreateContactRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetContactRequest), new { requestId = result.Id }, result);
    }

    /// <summary>
    /// Gets a specific contact request by ID.
    /// </summary>
    /// <param name="requestId">The contact request ID.</param>
    /// <returns>Contact request details.</returns>
    /// <response code="200">Returns contact request details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Contact request not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{requestId}")]
    [Authorize(Roles = "SuperUser,TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactRequestDto>> GetContactRequest(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetContactRequestsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var request = result.Requests.FirstOrDefault(r => r.Id == requestId);

        if (request == null)
            return NotFound();

        return Ok(request);
    }

    /// <summary>
    /// Resolves a contact request.
    /// </summary>
    /// <param name="requestId">The contact request ID.</param>
    /// <param name="command">Contact request resolution command.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Contact request resolved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Contact request not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{requestId}/resolve")]
    [Authorize(Roles = "SuperUser,TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResolveContactRequest(
        Guid requestId,
        [FromBody] ResolveContactRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        // Verify request exists
        var query = new GetContactRequestsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var request = result.Requests.FirstOrDefault(r => r.Id == requestId);

        if (request == null)
            return NotFound();

        // Set the request ID in the command
        command.ContactRequestId = requestId;
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
