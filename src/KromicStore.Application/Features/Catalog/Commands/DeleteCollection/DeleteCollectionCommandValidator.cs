using FluentValidation;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteCollection;

public sealed class DeleteCollectionCommandValidator : AbstractValidator<DeleteCollectionCommand>
{
    public DeleteCollectionCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty().WithMessage("CollectionId is required");
    }
}
