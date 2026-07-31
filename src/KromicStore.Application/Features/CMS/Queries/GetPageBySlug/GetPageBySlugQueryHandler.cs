using MediatR;
using KromicStore.Application.Features.CMS.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Queries.GetPageBySlug;

/// <summary>
/// Handler for GetPageBySlugQuery.
/// Retrieves a published CMS page by slug.
/// </summary>
public sealed class GetPageBySlugQueryHandler : IRequestHandler<GetPageBySlugQuery, PageDetailDto?>
{
    private readonly ICMSPageRepository _repository;
    private readonly ILogger<GetPageBySlugQueryHandler> _logger;

    public GetPageBySlugQueryHandler(
        ICMSPageRepository repository,
        ILogger<GetPageBySlugQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PageDetailDto?> Handle(GetPageBySlugQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving CMS page by slug: {Slug}", query.Slug);

        var page = await _repository.GetPublishedBySlugAsync(query.Slug, cancellationToken);

        if (page == null)
        {
            _logger.LogWarning("CMS page not found: {Slug}", query.Slug);
            return null;
        }

        var pageDto = new PageDetailDto(
            PageId: page.Id,
            Title: page.Title,
            Slug: page.Slug,
            Content: page.Content,
            MetaDescription: page.MetaDescription,
            MetaKeywords: page.MetaKeywords,
            Status: page.Status.ToString(),
            PublishedDateUtc: page.PublishedDateUtc,
            CreatedAtUtc: page.CreatedOnUtc);

        _logger.LogInformation("Retrieved CMS page: {Slug}", query.Slug);

        return pageDto;
    }
}
