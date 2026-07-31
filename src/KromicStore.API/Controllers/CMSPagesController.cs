using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.CMS;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for CMS page management.
/// Tenants can create, edit, and publish pages like About, Privacy Policy, Terms, etc.
/// </summary>
[ApiController]
[Route("api/v1/pages")]
public class CMSPagesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CMSPagesController"/> class.
    /// </summary>
    public CMSPagesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all published pages (customer-facing).
    /// </summary>
    /// <returns>List of published pages.</returns>
    /// <response code="200">Returns list of pages.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PageDto>>> GetPages(CancellationToken cancellationToken = default)
    {
        return Ok(Enumerable.Empty<PageDto>());
    }

    /// <summary>
    /// Gets a specific page by slug (customer-facing).
    /// </summary>
    /// <param name="slug">The page slug.</param>
    /// <returns>Page details.</returns>
    /// <response code="200">Returns page details.</response>
    /// <response code="404">Page not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PageDto>> GetPageBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return NotFound();
    }

    /// <summary>
    /// Creates a new CMS page (admin only).
    /// </summary>
    /// <param name="request">Page creation request.</param>
    /// <returns>Created page.</returns>
    /// <response code="201">Page created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="409">Slug already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PageDto>> CreatePage(
        [FromBody] CreatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        var pageDto = new PageDto
        {
            PageId = Guid.NewGuid(),
            Title = request.Title,
            Slug = request.Slug.ToLower().Replace(" ", "-"),
            Content = request.Content,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            IsPublished = request.Publish,
            PublishDate = request.Publish ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetPageBySlug), new { slug = pageDto.Slug }, pageDto);
    }

    /// <summary>
    /// Updates an existing page (admin only).
    /// </summary>
    /// <param name="pageId">The page ID.</param>
    /// <param name="request">Page update request.</param>
    /// <returns>Updated page.</returns>
    /// <response code="200">Page updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Page not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{pageId}")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PageDto>> UpdatePage(
        Guid pageId,
        [FromBody] UpdatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        return NotFound();
    }

    /// <summary>
    /// Deletes a page (admin only).
    /// </summary>
    /// <param name="pageId">The page ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Page deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Page not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{pageId}")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePage(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        return NoContent();
    }

    /// <summary>
    /// Publishes a draft page (admin only).
    /// </summary>
    /// <param name="pageId">The page ID.</param>
    /// <returns>Published page.</returns>
    /// <response code="200">Page published successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Page not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{pageId}/publish")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PageDto>> PublishPage(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        return NotFound();
    }

    /// <summary>
    /// Unpublishes a page (admin only).
    /// </summary>
    /// <param name="pageId">The page ID.</param>
    /// <returns>Unpublished page.</returns>
    /// <response code="200">Page unpublished successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Page not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{pageId}/unpublish")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PageDto>> UnpublishPage(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        return NotFound();
    }

    /// <summary>
    /// Schedules a page to be published at a future date (admin only).
    /// </summary>
    /// <param name="pageId">The page ID.</param>
    /// <param name="publishDate">Scheduled publish date.</param>
    /// <returns>Scheduled page.</returns>
    /// <response code="200">Page scheduled successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Page not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{pageId}/schedule")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PageDto>> SchedulePage(
        Guid pageId,
        [FromQuery] DateTime publishDate,
        CancellationToken cancellationToken = default)
    {
        if (publishDate < DateTime.UtcNow)
            return BadRequest(new { message = "Publish date must be in the future." });

        return NotFound();
    }
}
