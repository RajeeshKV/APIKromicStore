using MediatR;

namespace KromicStore.Application.Features.CMS.Queries.GetPages;

/// <summary>
/// Query to retrieve all published CMS pages.
/// </summary>
public sealed record GetPagesQuery(
    Guid TenantId,
    int Skip = 0,
    int Take = 50) : IRequest<IEnumerable<PageDto>>;

/// <summary>
/// DTO for a CMS page.
/// </summary>
public sealed record PageDto(
    Guid PageId,
    string Title,
    string Slug,
    string Content,
    string? MetaDescription,
    string? MetaKeywords,
    string Status,
    DateTime? PublishedDateUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
