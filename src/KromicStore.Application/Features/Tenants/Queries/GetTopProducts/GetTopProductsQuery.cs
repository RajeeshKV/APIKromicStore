using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetTopProducts;

public sealed class GetTopProductsQuery : IRequest<TopProductsResponse>
{
    public Guid TenantId { get; set; }
    public int Limit { get; set; } = 5;
}

public sealed class TopProductsResponse
{
    public List<TopProduct> Products { get; set; } = new();
}

public sealed class TopProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
}
