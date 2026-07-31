namespace KromicStore.Application.Features.Tenants.Queries.GetStoreOrders;

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    DateTime OrderDateUtc,
    decimal Total,
    string Status,
    int ItemCount);
