using MediatR;

namespace KromicStore.Application.Catalog.Commands.DeleteCategory;

/// <summary>
/// Command to delete (soft delete) a product category.
/// </summary>
public sealed class DeleteCategoryCommand : IRequest<DeleteCategoryResponse>
{
    public Guid CategoryId { get; set; }
}

public sealed class DeleteCategoryResponse
{
    public Guid CategoryId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime DeletedOnUtc { get; set; }
}
