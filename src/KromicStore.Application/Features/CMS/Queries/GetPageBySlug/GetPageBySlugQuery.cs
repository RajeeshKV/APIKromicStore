using MediatR;

namespace KromicStore.Application.Features.CMS.Queries.GetPageBySlug;

/// <summary>
/// Query to retrieve a published CMS page by slug.
/// </summary>
public sealed record GetPageBySlugQuery(
    string Slug) : IRequest<PageDetailDto?>;

/// <summary>
/// DTO for page detail view.
/// </summary>
public sealed record PageDetailDto(
    Guid PageId,
    string Title,
    string Slug,
    string Content,
    string? MetaDescription,
    string? MetaKeywords,
    string Status,
    DateTime? PublishedDateUtc,
    DateTime CreatedAtUtc);
