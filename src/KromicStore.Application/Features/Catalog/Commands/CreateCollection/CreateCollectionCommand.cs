using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.CreateCollection;

public sealed record CreateCollectionCommand(
    string Name,
    string? Description = null,
    int DisplayOrder = 0,
    string? Status = "Active") : IRequest<CreateCollectionResponse>;

public sealed record CreateCollectionResponse(
    Guid CollectionId,
    string Name,
    string Status);
