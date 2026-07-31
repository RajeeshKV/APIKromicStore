using MediatR;

namespace KromicStore.Application.Catalog.Queries.GetProducts;

/// <summary>
/// Query to retrieve products with pagination, filtering, and sorting.
/// </summary>
public sealed class GetProductsQuery : IRequest<GetProductsResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public string? SortBy { get; set; } // name, price, newest, popularity
    public bool? IsActive { get; set; }
}

public sealed class GetProductsResponse
{
    public List<ProductDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public sealed class ProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int Quantity { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int VariantCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
