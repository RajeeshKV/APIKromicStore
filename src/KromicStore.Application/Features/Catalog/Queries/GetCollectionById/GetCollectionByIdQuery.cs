using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetCollectionById;

/// <summary>
/// Query to retrieve a single collection by ID with product count.
/// </summary>
public sealed record GetCollectionByIdQuery(Guid CollectionId) : IRequest<GetCollectionByIdResponse>;

/// <summary>
/// Data transfer object for collection in query response.
/// </summary>
public sealed record CollectionDto(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    bool IsActive,
    int ProductCount,
    DateTime CreatedAtUtc);

/// <summary>
/// Response for GetCollectionById query.
/// </summary>
public sealed record GetCollectionByIdResponse(CollectionDto? Data);
