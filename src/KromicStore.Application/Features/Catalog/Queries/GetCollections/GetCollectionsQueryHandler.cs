using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetCollections;

public sealed class GetCollectionsQueryHandler : IRequestHandler<GetCollectionsQuery, GetCollectionsResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger<GetCollectionsQueryHandler> _logger;

    public GetCollectionsQueryHandler(
        ICollectionRepository collectionRepository,
        ILogger<GetCollectionsQueryHandler> logger)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetCollectionsResponse> Handle(
        GetCollectionsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving collections: Skip={Skip}, Take={Take}, IsActive={IsActive}",
            query.Skip, query.Take, query.IsActive);

        // EF global query filter already scopes results to the current tenant.
        var allCollections = await _collectionRepository.GetAllAsync(cancellationToken);

        var collections = allCollections
            .Where(c => !c.IsDeleted)
            .Where(c => query.IsActive == null || (query.IsActive.Value && c.Status == 0) || (!query.IsActive.Value && c.Status != 0))
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(MapToCollectionDto)
            .ToList();

        _logger.LogInformation("Retrieved {Count} collections", collections.Count);

        return new GetCollectionsResponse(collections);
    }

    private static CollectionDto MapToCollectionDto(dynamic collection)
    {
        return new CollectionDto(
            Id: collection.Id,
            Name: collection.Name,
            Description: collection.Description,
            Slug: collection.Name.ToLower().Replace(" ", "-"),
            IsActive: collection.Status == 0,
            ProductCount: collection.ProductMappings?.Count ?? 0,
            CreatedAtUtc: collection.CreatedOnUtc);
    }
}
