using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetCollectionById;

public sealed class GetCollectionByIdQueryHandler : IRequestHandler<GetCollectionByIdQuery, GetCollectionByIdResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger<GetCollectionByIdQueryHandler> _logger;

    public GetCollectionByIdQueryHandler(
        ICollectionRepository collectionRepository,
        ILogger<GetCollectionByIdQueryHandler> logger)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetCollectionByIdResponse> Handle(
        GetCollectionByIdQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving collection: {CollectionId}", query.CollectionId);

        // EF global query filter already scopes to the current tenant.
        var collection = await _collectionRepository.GetByIdAsync(query.CollectionId, cancellationToken);

        if (collection == null || collection.IsDeleted)
        {
            _logger.LogWarning("Collection not found or deleted: {CollectionId}", query.CollectionId);
            return new GetCollectionByIdResponse(null);
        }

        _logger.LogInformation("Collection retrieved successfully: {CollectionId}", query.CollectionId);
        return new GetCollectionByIdResponse(MapToCollectionDto(collection));
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
