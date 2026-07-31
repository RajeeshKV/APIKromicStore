using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetLowStockProducts;

/// <summary>
/// Query to retrieve products below stock threshold.
/// </summary>
public sealed class GetLowStockProductsQuery : IRequest<GetLowStockProductsResponse>
{
    public Guid TenantId { get; set; }
    public int Threshold { get; set; } = 10;
}

public sealed class LowStockProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
}

public sealed class GetLowStockProductsResponse
{
    public List<LowStockProductDto> Products { get; set; } = [];
}
