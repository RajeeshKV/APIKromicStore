using MediatR;

namespace KromicStore.Application.Catalog.Commands.CreateCategory;

/// <summary>
/// Command to create a new product category.
/// </summary>
public sealed class CreateCategoryCommand : IRequest<CreateCategoryResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CreateCategoryResponse
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
}
