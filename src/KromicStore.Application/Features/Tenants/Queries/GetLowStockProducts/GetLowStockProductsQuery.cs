using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetLowStockProducts;

public sealed class GetLowStockProductsQuery : IRequest<LowStockProductsResponse>
{
    public Guid TenantId { get; set; }
    public int ThresholdQty { get; set; } = 10;
}

public sealed class LowStockProductsResponse
{
    public List<LowStockProduct> Products { get; set; } = new();
}

public sealed class LowStockProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ThresholdQty { get; set; }
}
