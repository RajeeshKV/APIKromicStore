using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.CreateVariant;

public sealed record CreateVariantCommand(
    Guid ProductId,
    string SkuSuffix,
    string Name,
    decimal PriceAdjustment = 0,
    int StockQuantity = 0,
    Dictionary<string, string>? Attributes = null) : IRequest<CreateVariantResponse>;

public sealed record CreateVariantResponse(
    Guid VariantId,
    Guid ProductId,
    string Name,
    string Sku);
