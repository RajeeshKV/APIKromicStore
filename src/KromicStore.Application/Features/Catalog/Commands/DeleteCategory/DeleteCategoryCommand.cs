using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(
    Guid CategoryId) : IRequest<DeleteCategoryResponse>;

public sealed record DeleteCategoryResponse(
    Guid CategoryId,
    string Message);
