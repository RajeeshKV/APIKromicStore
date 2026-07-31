using MediatR;

namespace KromicStore.Application.Catalog.Commands.UpdateProduct;

/// <summary>
/// Command to update an existing product.
/// </summary>
public sealed class UpdateProductCommand : IRequest<UpdateProductResponse>
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; }
}

public sealed class UpdateProductResponse
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
}
