using FluentValidation;

namespace KromicStore.Application.Catalog.Commands.CreateCategory;

/// <summary>
/// Validator for CreateCategoryCommand.
/// </summary>
public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Category description must not exceed 500 characters");

        RuleFor(x => x.Slug)
            .MaximumLength(150).WithMessage("Category slug must not exceed 150 characters")
            .Matches(@"^[a-z0-9-]*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(60).WithMessage("Meta title must not exceed 60 characters");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160).WithMessage("Meta description must not exceed 160 characters");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(200).WithMessage("Meta keywords must not exceed 200 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }
}
