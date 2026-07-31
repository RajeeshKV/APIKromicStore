using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.CMS.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Commands.DeletePage;

/// <summary>
/// Handler for DeletePageCommand.
/// Soft-deletes a CMS page.
/// </summary>
public sealed class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, DeletePageResponse>
{
    private readonly ICMSPageRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeletePageCommandHandler> _logger;

    public DeletePageCommandHandler(
        ICMSPageRepository repository,
        IApplicationDbContext dbContext,
        ILogger<DeletePageCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DeletePageResponse> Handle(DeletePageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting CMS page: PageId={PageId}", command.PageId);

        // Get the page
        var page = await _repository.GetByIdAsync(command.PageId, cancellationToken);
        if (page == null)
        {
            _logger.LogWarning("Page not found: {PageId}", command.PageId);
            return new DeletePageResponse(
                PageId: command.PageId,
                Success: false,
                Message: "Page not found");
        }

        // Verify tenant ownership
        if (page.TenantId != command.TenantId)
        {
            _logger.LogWarning("Unauthorized access to page: {PageId}", command.PageId);
            return new DeletePageResponse(
                PageId: command.PageId,
                Success: false,
                Message: "Unauthorized");
        }

        // Soft-delete the page
        page.SoftDelete();
        _repository.Update(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS page deleted successfully: PageId={PageId}", command.PageId);

        return new DeletePageResponse(
            PageId: command.PageId,
            Success: true,
            Message: "Page deleted successfully");
    }
}
