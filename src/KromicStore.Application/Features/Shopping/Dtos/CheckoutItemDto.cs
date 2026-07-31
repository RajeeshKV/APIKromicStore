namespace KromicStore.Application.Features.Shopping.Dtos;

/// <summary>
/// DTO for a checkout item in responses.
/// </summary>
public sealed record CheckoutItemDto(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
