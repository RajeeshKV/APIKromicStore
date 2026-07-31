using FluentValidation;

namespace KromicStore.Application.Features.Promotions.Commands.CreateCampaign;

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Campaign name is required")
            .MaximumLength(200)
            .WithMessage("Campaign name cannot exceed 200 characters");
        
        RuleFor(x => x.StartDateUtc)
            .NotEmpty()
            .WithMessage("Start date is required")
            .LessThan(x => x.EndDateUtc)
            .WithMessage("Start date must be before end date");
        
        RuleFor(x => x.EndDateUtc)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDateUtc)
            .WithMessage("End date must be after start date");
        
        RuleFor(x => x.DiscountIds)
            .NotEmpty()
            .WithMessage("At least one discount is required");
    }
}
