using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.RestoreCategory;

public sealed record RestoreCategoryCommand(
    Guid CategoryId) : IRequest<RestoreCategoryResponse>;

public sealed record RestoreCategoryResponse(
    Guid CategoryId,
    string Message);
