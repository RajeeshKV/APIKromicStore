using FluentValidation;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Commands.CreateCollection;

public sealed class CreateCollectionCommandValidator : AbstractValidator<CreateCollectionCommand>
{
    private readonly ICollectionRepository _collectionRepository;

    public CreateCollectionCommandValidator(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be non-negative");

        RuleFor(x => x.Status)
            .Must(x => x == "Active" || x == "Archived").WithMessage("Status must be either 'Active' or 'Archived'")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}
