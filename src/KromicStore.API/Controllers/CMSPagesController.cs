using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.CMS.Commands.CreatePage;
using KromicStore.Application.Features.CMS.Commands.UpdatePage;
using KromicStore.Application.Features.CMS.Commands.DeletePage;
using KromicStore.Application.Features.CMS.Commands.PublishPage;
using KromicStore.Application.Features.CMS.Commands.UnpublishPage;
using KromicStore.Application.Features.CMS.Commands.SchedulePage;
using KromicStore.Application.Features.CMS.Queries.GetPages;
using KromicStore.Application.Features.CMS.Queries.GetPageBySlug;
using KromicStore.Application.Common.Abstractions;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for CMS page management.
/// Tenants can create, edit, publish, and manage pages.
/// </summary>
[ApiController]
[Route("api/v1/pages")]
[Produces("application/json")]
public class CMSPagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CMSPagesController> _logger;

    public CMSPagesController(
        IMediator mediator,
        ITenantContext tenantContext,
        ILogger<CMSPagesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all published pages (customer-facing).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PageDto>>> GetPages(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPagesQuery(_tenantContext.TenantId ?? Guid.Empty, skip, take);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pages");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a specific page by slug (customer-facing).
    /// </summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PageDetailDto>> GetPageBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPageBySlugQuery(slug);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result == null)
                return NotFound(new { message = "Page not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving page: {Slug}", slug);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Creates a new CMS page (admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreatePageResponse>> CreatePage(
        [FromBody] CreatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
                return Unauthorized(new { message = "Tenant context not resolved" });

            var command = new CreatePageCommand(
                TenantId: tenantId.Value,
                Title: request.Title,
                Slug: request.Slug,
                Content: request.Content,
                MetaDescription: request.MetaDescription,
                MetaKeywords: request.MetaKeywords,
                Publish: request.Publish ?? false);

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetPageBySlug), new { slug = result.Slug }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Page creation failed");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating page");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates an existing page (admin only).
    /// </summary>
    [HttpPut("{pageId}")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UpdatePageResponse>> UpdatePage(
        Guid pageId,
        [FromBody] UpdatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
                return Unauthorized(new { message = "Tenant context not resolved" });

            var command = new UpdatePageCommand(
                PageId: pageId,
                TenantId: tenantId.Value,
                Title: request.Title,
                Slug: request.Slug,
                Content: request.Content,
                MetaDescription: request.MetaDescription,
                MetaKeywords: request.MetaKeywords);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Page update failed");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized page update");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a page (admin only).
    /// </summary>
    [HttpDelete("{pageId}")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePage(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
                return Unauthorized(new { message = "Tenant context not resolved" });

            var command = new DeletePageCommand(pageId, tenantId.Value);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting page");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Publishes a draft page immediately (admin only).
    /// </summary>
    [HttpPost("{pageId}/publish")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PublishPageResponse>> PublishPage(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
                return Unauthorized(new { message = "Tenant context not resolved" });

            var command = new PublishPageCommand(pageId, tenantId.Value);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Page publish failed");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized page publish");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing page");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Unpublishes a page (admin only).
    /// </summary>
    [HttpPost("{pageId}/unpublish")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UnpublishPageResponse>> UnpublishPage(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
                return Unauthorized(new { message = "Tenant context not resolved" });

            var command = new UnpublishPageCommand(pageId, tenantId.Value);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Page unpublish failed");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized page unpublish");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing page");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Schedules a page for publication at a future date (admin only).
    /// </summary>
    [HttpPost("{pageId}/schedule")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SchedulePageResponse>> SchedulePage(
        Guid pageId,
        [FromQuery] DateTime publishDateUtc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (publishDateUtc <= DateTime.UtcNow)
                return BadRequest(new { message = "Publish date must be in the future" });

            var tenantId = _tenantContext.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
                return Unauthorized(new { message = "Tenant context not resolved" });

            var command = new SchedulePageCommand(pageId, tenantId.Value, publishDateUtc);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Page schedule failed");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized page schedule");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling page");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

// ── Request/Response DTOs ───────────────────────────────────────────

public record CreatePageRequest(
    string Title,
    string Slug,
    string Content,
    string? MetaDescription = null,
    string? MetaKeywords = null,
    bool? Publish = false);

public record UpdatePageRequest(
    string Title,
    string Slug,
    string Content,
    string? MetaDescription = null,
    string? MetaKeywords = null);
