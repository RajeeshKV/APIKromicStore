using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetCategoryById;

/// <summary>
/// Query to retrieve a single category by ID.
/// </summary>
public sealed record GetCategoryByIdQuery(Guid CategoryId) : IRequest<GetCategoryByIdResponse>;

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
/// Response for GetCategoryById query.
/// </summary>
public sealed record GetCategoryByIdResponse(CategoryDto? Data);
