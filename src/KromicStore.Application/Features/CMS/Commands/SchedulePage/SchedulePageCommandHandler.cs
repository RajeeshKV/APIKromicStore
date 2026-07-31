using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.CMS.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Commands.SchedulePage;

/// <summary>
/// Handler for SchedulePageCommand.
/// Schedules a CMS page for future publication.
/// </summary>
public sealed class SchedulePageCommandHandler : IRequestHandler<SchedulePageCommand, SchedulePageResponse>
{
    private readonly ICMSPageRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<SchedulePageCommandHandler> _logger;

    public SchedulePageCommandHandler(
        ICMSPageRepository repository,
        IApplicationDbContext dbContext,
        ILogger<SchedulePageCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SchedulePageResponse> Handle(SchedulePageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduling CMS page: PageId={PageId}, PublishDate={PublishDate}", command.PageId, command.PublishDateUtc);

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

        // Schedule the page
        page.Schedule(command.PublishDateUtc);
        _repository.Update(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS page scheduled successfully: PageId={PageId}, ScheduledDate={ScheduledDate}", command.PageId, page.ScheduledPublishDateUtc);

        return new SchedulePageResponse(
            PageId: page.Id,
            Status: page.Status.ToString(),
            ScheduledPublishDateUtc: page.ScheduledPublishDateUtc);
    }
}
