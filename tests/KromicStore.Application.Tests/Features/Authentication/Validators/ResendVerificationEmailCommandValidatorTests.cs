using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.ResendVerificationEmail;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class ResendVerificationEmailCommandValidatorTests
{
    private readonly ResendVerificationEmailCommandValidator _sut = new();

    [Fact]
    public void Email_ShouldFail_WhenEmpty()
        => _sut.TestValidate(new ResendVerificationEmailCommand(""))
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void Email_ShouldFail_WhenInvalidFormat(string email)
        => _sut.TestValidate(new ResendVerificationEmailCommand(email))
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Validate_ShouldPass_WhenEmailValid()
        => _sut.TestValidate(new ResendVerificationEmailCommand("alice@example.com"))
               .IsValid.Should().BeTrue();
}
