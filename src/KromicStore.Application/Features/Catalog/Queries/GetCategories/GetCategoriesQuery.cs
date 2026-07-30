using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetCategories;

/// <summary>
/// Query to retrieve all non-deleted categories with optional filtering and pagination.
/// </summary>
public sealed record GetCategoriesQuery(
    int Skip = 0,
    int Take = 10,
    Guid? ParentCategoryId = null) : IRequest<GetCategoriesResponse>;

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
/// Response for GetCategories query.
/// </summary>
public sealed record GetCategoriesResponse(IEnumerable<CategoryDto> Data);
