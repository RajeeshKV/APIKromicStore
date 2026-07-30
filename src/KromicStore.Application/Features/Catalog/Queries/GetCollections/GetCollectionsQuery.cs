using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetCollections;

/// <summary>
/// Query to retrieve all non-deleted collections with optional filtering and pagination.
/// </summary>
public sealed record GetCollectionsQuery(
    int Skip = 0,
    int Take = 10,
    bool? IsActive = null) : IRequest<GetCollectionsResponse>;

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
/// Response for GetCollections query.
/// </summary>
public sealed record GetCollectionsResponse(IEnumerable<CollectionDto> Data);
