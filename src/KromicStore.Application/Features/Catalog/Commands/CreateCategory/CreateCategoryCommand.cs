using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description = null,
    string? Slug = null,
    Guid? ParentCategoryId = null,
    int DisplayOrder = 0,
    bool IsVisible = true,
    string? ImageUrl = null) : IRequest<CreateCategoryResponse>;

public sealed record CreateCategoryResponse(
    Guid CategoryId,
    string Name,
    string Slug);
