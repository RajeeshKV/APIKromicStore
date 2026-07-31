using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Tenants.Queries.GetSubscriptionPlans;
using KromicStore.Application.Features.Tenants.Commands.CreateSubscriptionPlan;
using KromicStore.Application.Features.Tenants.Commands.UpdateSubscriptionPlan;
using KromicStore.Application.Features.Tenants.Commands.DeleteSubscriptionPlan;
using KromicStore.Application.Features.Tenants.Commands.ActivateSubscriptionPlan;
using KromicStore.Application.Features.Tenants.Commands.DeactivateSubscriptionPlan;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for subscription plan management.
/// SuperUsers can create, manage, and configure subscription plans.
/// </summary>
[ApiController]
[Route("api/v1/subscription-plans")]
[Authorize(Roles = "SuperUser")]
public class SubscriptionPlanController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionPlanController"/> class.
    /// </summary>
    public SubscriptionPlanController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all subscription plans.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20).</param>
    /// <param name="isActive">Filter by active status (optional).</param>
    /// <returns>List of subscription plans.</returns>
    /// <response code="200">Returns list of subscription plans.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionPlansResponse>> GetSubscriptionPlans(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSubscriptionPlansQuery
        {
            Skip = skip,
            Take = take,
            IsActive = isActive
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new subscription plan.
    /// </summary>
    /// <param name="command">Subscription plan creation command.</param>
    /// <returns>Created subscription plan.</returns>
    /// <response code="201">Subscription plan created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateSubscriptionPlanResponse>> CreateSubscriptionPlan(
        [FromBody] CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSubscriptionPlan), new { planId = result.Id }, result);
    }

    /// <summary>
    /// Gets a specific subscription plan by ID.
    /// </summary>
    /// <param name="planId">The subscription plan ID.</param>
    /// <returns>Subscription plan details.</returns>
    /// <response code="200">Returns subscription plan details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Subscription plan not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{planId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionPlanDto>> GetSubscriptionPlan(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var plan = result.Plans.FirstOrDefault(p => p.Id == planId);

        if (plan == null)
            return NotFound();

        return Ok(plan);
    }

    /// <summary>
    /// Updates an existing subscription plan.
    /// </summary>
    /// <param name="planId">The subscription plan ID.</param>
    /// <param name="command">Subscription plan update command.</param>
    /// <returns>Updated subscription plan.</returns>
    /// <response code="200">Subscription plan updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Subscription plan not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{planId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionPlanDto>> UpdateSubscriptionPlan(
        Guid planId,
        [FromBody] UpdateSubscriptionPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        // Verify plan exists
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var plan = result.Plans.FirstOrDefault(p => p.Id == planId);

        if (plan == null)
            return NotFound();

        // Set the plan ID in the command
        command.PlanId = planId;
        await _mediator.Send(command, cancellationToken);

        return Ok(plan);
    }

    /// <summary>
    /// Deletes a subscription plan.
    /// </summary>
    /// <param name="planId">The subscription plan ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Subscription plan deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Subscription plan not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{planId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSubscriptionPlan(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        // Verify plan exists
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var plan = result.Plans.FirstOrDefault(p => p.Id == planId);

        if (plan == null)
            return NotFound();

        var command = new DeleteSubscriptionPlanCommand { PlanId = planId };
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Activates a subscription plan.
    /// </summary>
    /// <param name="planId">The subscription plan ID.</param>
    /// <returns>Activated subscription plan.</returns>
    /// <response code="200">Subscription plan activated successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Subscription plan not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{planId}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionPlanDto>> ActivateSubscriptionPlan(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        // Verify plan exists
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var plan = result.Plans.FirstOrDefault(p => p.Id == planId);

        if (plan == null)
            return NotFound();

        var command = new ActivateSubscriptionPlanCommand { PlanId = planId };
        await _mediator.Send(command, cancellationToken);

        return Ok(plan);
    }

    /// <summary>
    /// Deactivates a subscription plan.
    /// </summary>
    /// <param name="planId">The subscription plan ID.</param>
    /// <returns>Deactivated subscription plan.</returns>
    /// <response code="200">Subscription plan deactivated successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Subscription plan not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{planId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionPlanDto>> DeactivateSubscriptionPlan(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        // Verify plan exists
        var query = new GetSubscriptionPlansQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var plan = result.Plans.FirstOrDefault(p => p.Id == planId);

        if (plan == null)
            return NotFound();

        var command = new DeactivateSubscriptionPlanCommand { PlanId = planId };
        await _mediator.Send(command, cancellationToken);

        return Ok(plan);
    }
}
