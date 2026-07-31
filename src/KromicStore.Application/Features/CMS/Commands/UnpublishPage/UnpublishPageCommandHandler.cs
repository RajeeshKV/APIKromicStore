using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.CMS.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Commands.UnpublishPage;

/// <summary>
/// Handler for UnpublishPageCommand.
/// Unpublishes a CMS page (returns to draft).
/// </summary>
public sealed class UnpublishPageCommandHandler : IRequestHandler<UnpublishPageCommand, UnpublishPageResponse>
{
    private readonly ICMSPageRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UnpublishPageCommandHandler> _logger;

    public UnpublishPageCommandHandler(
        ICMSPageRepository repository,
        IApplicationDbContext dbContext,
        ILogger<UnpublishPageCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UnpublishPageResponse> Handle(UnpublishPageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unpublishing CMS page: PageId={PageId}", command.PageId);

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

        // Unpublish the page
        page.Unpublish();
        _repository.Update(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS page unpublished successfully: PageId={PageId}", command.PageId);

        return new UnpublishPageResponse(
            PageId: page.Id,
            Status: page.Status.ToString());
    }
}
