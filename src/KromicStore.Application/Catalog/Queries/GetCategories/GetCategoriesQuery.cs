using MediatR;

namespace KromicStore.Application.Catalog.Queries.GetCategories;

/// <summary>
/// Query to retrieve categories with pagination and filtering.
/// </summary>
public sealed class GetCategoriesQuery : IRequest<GetCategoriesResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentCategoryId { get; set; }
}

public sealed class GetCategoriesResponse
{
    public List<CategoryDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public sealed class CategoryDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
