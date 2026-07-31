using MediatR;

namespace KromicStore.Application.Catalog.Queries.GetInventoryStatus;

/// <summary>
/// Query to retrieve inventory status for products.
/// </summary>
public sealed class GetInventoryStatusQuery : IRequest<GetInventoryStatusResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? StatusFilter { get; set; } // all, low-stock, out-of-stock
    public Guid? CategoryId { get; set; }
    public int LowStockThreshold { get; set; } = 10;
}

public sealed class GetInventoryStatusResponse
{
    public List<InventoryStatusDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }

    public sealed class InventoryStatusDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string Status { get; set; } = string.Empty; // in-stock, low-stock, out-of-stock
        public decimal Price { get; set; }
        public DateTime LastAdjustedOnUtc { get; set; }
    }
}
