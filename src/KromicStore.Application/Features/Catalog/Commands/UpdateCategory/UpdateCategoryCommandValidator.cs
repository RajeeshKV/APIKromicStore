using FluentValidation;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("Slug cannot exceed 100 characters")
            .Matches(@"^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("ImageUrl cannot exceed 500 characters")
            .When(x => x.ImageUrl is not null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be non-negative")
            .When(x => x.DisplayOrder.HasValue);

        RuleFor(x => x.ParentCategoryId)
            .NotEqual(x => x.CategoryId).WithMessage("A category cannot be its own parent")
            .When(x => x.ParentCategoryId.HasValue);
    }
}
