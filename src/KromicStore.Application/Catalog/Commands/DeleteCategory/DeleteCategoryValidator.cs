using FluentValidation;

namespace KromicStore.Application.Catalog.Commands.DeleteCategory;

/// <summary>
/// Validator for DeleteCategoryCommand.
/// </summary>
public sealed class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("Category ID is required");
    }
}
