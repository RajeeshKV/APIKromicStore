using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.CMS.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Commands.PublishPage;

/// <summary>
/// Handler for PublishPageCommand.
/// Publishes a CMS page immediately.
/// </summary>
public sealed class PublishPageCommandHandler : IRequestHandler<PublishPageCommand, PublishPageResponse>
{
    private readonly ICMSPageRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<PublishPageCommandHandler> _logger;

    public PublishPageCommandHandler(
        ICMSPageRepository repository,
        IApplicationDbContext dbContext,
        ILogger<PublishPageCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PublishPageResponse> Handle(PublishPageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing CMS page: PageId={PageId}", command.PageId);

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

        // Publish the page
        page.Publish();
        _repository.Update(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS page published successfully: PageId={PageId}", command.PageId);

        return new PublishPageResponse(
            PageId: page.Id,
            Status: page.Status.ToString(),
            PublishedDateUtc: page.PublishedDateUtc);
    }
}
