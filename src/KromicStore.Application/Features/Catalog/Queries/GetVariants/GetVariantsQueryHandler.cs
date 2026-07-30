using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetVariants;

public sealed class GetVariantsQueryHandler : IRequestHandler<GetVariantsQuery, GetVariantsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetVariantsQueryHandler> _logger;

    public GetVariantsQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        ILogger<GetVariantsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetVariantsResponse> Handle(
        GetVariantsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving variants for product: {ProductId}", query.ProductId);

        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetVariantsResponse([]);
        }

        if (product.TenantId != _tenantContext.TenantId)
        {
            _logger.LogWarning("Unauthorized access to product: {ProductId}", query.ProductId);
            throw new UnauthorizedAccessException($"Not authorized to access this resource.");
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
                    CostPrice: null, // Not available in variant data
                    Attributes: v.Attributes != null
                        ? v.Attributes.ToDictionary(a => a.Name, a => a.Value)
                        : [],
                    QuantityOnHand: v.StockQuantity,
                    IsAvailable: v.IsActive && v.StockQuantity > 0,
                    CreatedAtUtc: DateTime.UtcNow)) // Note: ProductVariant doesn't track CreatedAtUtc; using current time
                .ToList();
        }

        _logger.LogInformation("Retrieved {Count} variants for product: {ProductId}", variants.Count, query.ProductId);

        return new GetVariantsResponse(variants);
    }
}
