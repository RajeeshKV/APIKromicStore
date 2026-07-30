using FluentValidation;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("Slug cannot exceed 100 characters")
            .Matches(@"^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("ImageUrl cannot exceed 500 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be non-negative");
    }
}
