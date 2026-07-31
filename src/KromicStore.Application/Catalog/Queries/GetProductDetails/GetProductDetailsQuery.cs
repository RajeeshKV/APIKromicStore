using MediatR;

namespace KromicStore.Application.Catalog.Queries.GetProductDetails;

/// <summary>
/// Query to retrieve detailed product information including variants and images.
/// </summary>
public sealed class GetProductDetailsQuery : IRequest<GetProductDetailsResponse>
{
    public Guid ProductId { get; set; }
}

public sealed class GetProductDetailsResponse
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
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductAttributeDto> Attributes { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }

    public sealed class ProductImageDto
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }

    public sealed class ProductVariantDto
    {
        public Guid VariantId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public sealed class ProductAttributeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
