using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetCollectionById;

public sealed class GetCollectionByIdQueryHandler : IRequestHandler<GetCollectionByIdQuery, GetCollectionByIdResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetCollectionByIdQueryHandler> _logger;

    public GetCollectionByIdQueryHandler(
        ICollectionRepository collectionRepository,
        ITenantContext tenantContext,
        ILogger<GetCollectionByIdQueryHandler> logger)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetCollectionByIdResponse> Handle(
        GetCollectionByIdQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving collection: {CollectionId}", query.CollectionId);

        var collection = await _collectionRepository.GetByIdAsync(query.CollectionId, cancellationToken);

        if (collection == null || collection.IsDeleted)
        {
            _logger.LogWarning("Collection not found or deleted: {CollectionId}", query.CollectionId);
            return new GetCollectionByIdResponse(null);
        }

        if (collection.TenantId != _tenantContext.TenantId)
        {
            _logger.LogWarning("Unauthorized access to collection: {CollectionId}", query.CollectionId);
            throw new UnauthorizedAccessException($"Not authorized to access this resource.");
        }

        var collectionDto = MapToCollectionDto(collection);
        _logger.LogInformation("Collection retrieved successfully: {CollectionId}", query.CollectionId);

        return new GetCollectionByIdResponse(collectionDto);
    }

    private static CollectionDto MapToCollectionDto(dynamic collection)
    {
        return new CollectionDto(
            Id: collection.Id,
            Name: collection.Name,
            Description: collection.Description,
            Slug: collection.Name.ToLower().Replace(" ", "-"),
            IsActive: collection.Status == 0, // CollectionStatus.Active
            ProductCount: collection.ProductMappings?.Count ?? 0,
            CreatedAtUtc: collection.CreatedAtUtc);
    }
}
