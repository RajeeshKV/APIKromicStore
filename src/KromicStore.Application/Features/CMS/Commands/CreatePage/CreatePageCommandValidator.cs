using FluentValidation;

namespace KromicStore.Application.Features.CMS.Commands.CreatePage;

/// <summary>
/// Validator for CreatePageCommand.
/// </summary>
public sealed class CreatePageCommandValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Page title is required")
            .MaximumLength(200).WithMessage("Page title must not exceed 200 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Page slug is required")
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Page content is required")
            .MaximumLength(50000).WithMessage("Page content must not exceed 50,000 characters");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160).WithMessage("Meta description must not exceed 160 characters");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(200).WithMessage("Meta keywords must not exceed 200 characters");
    }
}
