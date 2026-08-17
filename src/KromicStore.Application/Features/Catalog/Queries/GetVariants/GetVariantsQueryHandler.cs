using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetVariants;

public sealed class GetVariantsQueryHandler : IRequestHandler<GetVariantsQuery, GetVariantsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetVariantsQueryHandler> _logger;

    public GetVariantsQueryHandler(
        IProductRepository productRepository,
        ILogger<GetVariantsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetVariantsResponse> Handle(
        GetVariantsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving variants for product: {ProductId}", query.ProductId);

        // EF global query filter already scopes to the current tenant.
        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetVariantsResponse([]);
        }

        var variants = new List<VariantDto>();

        if (product.Variants != null && product.Variants.Count > 0)
        {
            variants = product.Variants
                .Select(v => new VariantDto(
                    Id: v.Id,
                    Sku: v.Sku,
                    Name: v.Name,
                    Price: v.PriceAdjustment > 0 ? product.Price + v.PriceAdjustment : null,
                    CostPrice: null,
                    Attributes: v.Attributes != null
                        ? v.Attributes.ToDictionary(a => a.Name, a => a.Value)
                        : [],
                    QuantityOnHand: v.StockQuantity,
                    IsAvailable: v.IsActive && v.StockQuantity > 0,
                    CreatedAtUtc: DateTime.UtcNow))
                .ToList();
        }

        _logger.LogInformation("Retrieved {Count} variants for product: {ProductId}", variants.Count, query.ProductId);
        return new GetVariantsResponse(variants);
    }
}
