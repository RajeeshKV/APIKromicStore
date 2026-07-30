using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.SearchCategories;

/// <summary>
/// Query to search categories by name and slug.
/// </summary>
public sealed record SearchCategoriesQuery(string SearchText) : IRequest<SearchCategoriesResponse>;

/// <summary>
/// Data transfer object for category in query response.
/// </summary>
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int DisplayOrder,
    bool IsActive,
    string Slug,
    int ProductCount,
    DateTime CreatedAtUtc,
    DateTime? ModifiedAtUtc);

/// <summary>
/// Response for SearchCategories query.
/// </summary>
public sealed record SearchCategoriesResponse(IEnumerable<CategoryDto> Data);
