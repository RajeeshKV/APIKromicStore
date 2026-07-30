using FluentValidation;

namespace KromicStore.Application.Features.Catalog.Commands.RestoreCategory;

public sealed class RestoreCategoryCommandValidator : AbstractValidator<RestoreCategoryCommand>
{
    public RestoreCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");
    }
}
