using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.CMS.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Commands.UpdatePage;

/// <summary>
/// Handler for UpdatePageCommand.
/// Updates an existing CMS page content.
/// </summary>
public sealed class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, UpdatePageResponse>
{
    private readonly ICMSPageRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdatePageCommandHandler> _logger;

    public UpdatePageCommandHandler(
        ICMSPageRepository repository,
        IApplicationDbContext dbContext,
        ILogger<UpdatePageCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdatePageResponse> Handle(UpdatePageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating CMS page: PageId={PageId}", command.PageId);

        // Get the page
        var page = await _repository.GetByIdAsync(command.PageId, cancellationToken);
        if (page == null)
        {
            _logger.LogWarning("Page not found: {PageId}", command.PageId);
            throw new InvalidOperationException($"Page with ID {command.PageId} not found");
        }

        // Verify tenant ownership
        if (page.TenantId != command.TenantId)
        {
            _logger.LogWarning("Unauthorized access to page: {PageId}", command.PageId);
            throw new UnauthorizedAccessException("Cannot access page from another tenant");
        }

        // Check if new slug already exists (if slug changed)
        if (page.Slug != command.Slug)
        {
            var slugExists = await _repository.SlugExistsAsync(
                command.TenantId,
                command.Slug,
                command.PageId,
                cancellationToken);

            if (slugExists)
            {
                _logger.LogWarning("Slug already exists: {Slug}", command.Slug);
                throw new InvalidOperationException($"A page with slug '{command.Slug}' already exists");
            }
        }

        // Update the page
        page.Update(
            command.Title,
            command.Slug,
            command.Content,
            command.MetaDescription,
            command.MetaKeywords);

        // Persist
        _repository.Update(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS page updated successfully: PageId={PageId}", command.PageId);

        return new UpdatePageResponse(
            PageId: page.Id,
            Title: page.Title,
            Slug: page.Slug,
            Status: page.Status.ToString(),
            UpdatedAtUtc: page.ModifiedOnUtc ?? DateTime.UtcNow);
    }
}
