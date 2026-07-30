using FluentValidation;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateCollection;

public sealed class UpdateCollectionCommandValidator : AbstractValidator<UpdateCollectionCommand>
{
    private readonly ICollectionRepository _collectionRepository;

    public UpdateCollectionCommandValidator(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));

        RuleFor(x => x.CollectionId)
            .NotEmpty().WithMessage("CollectionId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be non-negative")
            .When(x => x.DisplayOrder.HasValue);

        RuleFor(x => x.Status)
            .Must(x => x == "Active" || x == "Archived").WithMessage("Status must be either 'Active' or 'Archived'")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}
