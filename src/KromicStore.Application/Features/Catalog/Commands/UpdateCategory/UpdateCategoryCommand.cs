using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string? Name = null,
    string? Description = null,
    string? Slug = null,
    Guid? ParentCategoryId = null,
    int? DisplayOrder = null,
    bool? IsVisible = null,
    string? ImageUrl = null) : IRequest<UpdateCategoryResponse>;

public sealed record UpdateCategoryResponse(
    Guid CategoryId,
    string Name,
    string Slug);
