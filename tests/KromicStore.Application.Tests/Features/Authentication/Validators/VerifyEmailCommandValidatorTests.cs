using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.VerifyEmail;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class VerifyEmailCommandValidatorTests
{
    private readonly VerifyEmailCommandValidator _sut = new();

    [Fact]
    public void Token_ShouldFail_WhenEmpty()
        => _sut.TestValidate(new VerifyEmailCommand(""))
               .ShouldHaveValidationErrorFor(x => x.Token);

    [Fact]
    public void Token_ShouldFail_WhenWhitespace()
        => _sut.TestValidate(new VerifyEmailCommand("  "))
               .ShouldHaveValidationErrorFor(x => x.Token);

    [Fact]
    public void Validate_ShouldPass_WhenTokenPresent()
        => _sut.TestValidate(new VerifyEmailCommand("abc-token"))
               .IsValid.Should().BeTrue();
}
