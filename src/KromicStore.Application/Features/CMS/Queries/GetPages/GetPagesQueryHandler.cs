using MediatR;
using KromicStore.Application.Features.CMS.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Queries.GetPages;

/// <summary>
/// Handler for GetPagesQuery.
/// Retrieves all published CMS pages for a tenant.
/// </summary>
public sealed class GetPagesQueryHandler : IRequestHandler<GetPagesQuery, IEnumerable<PageDto>>
{
    private readonly ICMSPageRepository _repository;
    private readonly ILogger<GetPagesQueryHandler> _logger;

    public GetPagesQueryHandler(
        ICMSPageRepository repository,
        ILogger<GetPagesQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<PageDto>> Handle(GetPagesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving CMS pages: TenantId={TenantId}, Skip={Skip}, Take={Take}", 
            query.TenantId, query.Skip, query.Take);

        var pages = await _repository.GetPublishedPagesAsync(
            query.TenantId,
            query.Skip,
            query.Take,
            cancellationToken);

        var pageDtos = pages.Select(MapToDto).ToList();

        _logger.LogInformation("Retrieved {Count} CMS pages", pageDtos.Count);

        return pageDtos;
    }

    private static PageDto MapToDto(Domain.CMS.Entities.CMSPage page)
    {
        return new PageDto(
            PageId: page.Id,
            Title: page.Title,
            Slug: page.Slug,
            Content: page.Content,
            MetaDescription: page.MetaDescription,
            MetaKeywords: page.MetaKeywords,
            Status: page.Status.ToString(),
            PublishedDateUtc: page.PublishedDateUtc,
            CreatedAtUtc: page.CreatedOnUtc,
            UpdatedAtUtc: page.ModifiedOnUtc ?? DateTime.UtcNow);
    }
}
