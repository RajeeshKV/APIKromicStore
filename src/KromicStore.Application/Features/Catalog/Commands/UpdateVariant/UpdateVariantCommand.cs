using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateVariant;

public sealed record UpdateVariantCommand(
    Guid ProductId,
    Guid VariantId,
    string? Name = null,
    decimal? PriceAdjustment = null,
    Dictionary<string, string>? Attributes = null,
    bool? IsActive = null) : IRequest<UpdateVariantResponse>;

public sealed record UpdateVariantResponse(
    Guid VariantId,
    Guid ProductId,
    string Name,
    string Sku);
