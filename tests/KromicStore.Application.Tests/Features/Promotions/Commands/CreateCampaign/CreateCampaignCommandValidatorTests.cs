using FluentValidation.TestHelper;
using KromicStore.Application.Features.Promotions.Commands.CreateCampaign;
using Xunit;

namespace KromicStore.Application.Tests.Features.Promotions.Commands.CreateCampaign;

public sealed class CreateCampaignCommandValidatorTests
{
    private readonly CreateCampaignCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var now = DateTime.UtcNow;
        var command = new CreateCampaignCommand
        {
            Name = "Summer Campaign",
            Description = "Great summer deals",
            StartDateUtc = now,
            EndDateUtc = now.AddDays(30),
            DiscountIds = [Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var command = new CreateCampaignCommand
        {
            Name = string.Empty,
            StartDateUtc = DateTime.UtcNow,
            EndDateUtc = DateTime.UtcNow.AddDays(30),
            DiscountIds = [Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Campaign name is required");
    }

    [Fact]
    public void Validate_WithExcessivelyLongName_ShouldHaveError()
    {
        var command = new CreateCampaignCommand
        {
            Name = new string('a', 201),
            StartDateUtc = DateTime.UtcNow,
            EndDateUtc = DateTime.UtcNow.AddDays(30),
            DiscountIds = [Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Campaign name cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_WithEndBeforeStart_ShouldHaveError()
    {
        var now = DateTime.UtcNow;
        var command = new CreateCampaignCommand
        {
            Name = "Campaign",
            StartDateUtc = now.AddDays(30),
            EndDateUtc = now,
            DiscountIds = [Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartDateUtc)
            .WithErrorMessage("Start date must be before end date");
    }

    [Fact]
    public void Validate_WithEndEqualToStart_ShouldHaveError()
    {
        var now = DateTime.UtcNow;
        var command = new CreateCampaignCommand
        {
            Name = "Campaign",
            StartDateUtc = now,
            EndDateUtc = now,
            DiscountIds = [Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EndDateUtc)
            .WithErrorMessage("End date must be after start date");
    }

    [Fact]
    public void Validate_WithNoDiscounts_ShouldHaveError()
    {
        var command = new CreateCampaignCommand
        {
            Name = "Campaign",
            StartDateUtc = DateTime.UtcNow,
            EndDateUtc = DateTime.UtcNow.AddDays(30),
            DiscountIds = []
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DiscountIds)
            .WithErrorMessage("At least one discount is required");
    }

    [Fact]
    public void Validate_WithMultipleDiscounts_ShouldNotHaveErrors()
    {
        var now = DateTime.UtcNow;
        var command = new CreateCampaignCommand
        {
            Name = "Campaign",
            StartDateUtc = now,
            EndDateUtc = now.AddDays(30),
            DiscountIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOptionalDescription_ShouldNotHaveErrors()
    {
        var now = DateTime.UtcNow;
        var command = new CreateCampaignCommand
        {
            Name = "Campaign",
            Description = "Optional description",
            StartDateUtc = now,
            EndDateUtc = now.AddDays(30),
            DiscountIds = [Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithoutOptionalDescription_ShouldNotHaveErrors()
    {
        var now = DateTime.UtcNow;
        var command = new CreateCampaignCommand
        {
            Name = "Campaign",
            StartDateUtc = now,
            EndDateUtc = now.AddDays(30),
            DiscountIds = [Guid.NewGuid()]
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
