using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.ForgotPassword;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _sut = new();

    [Fact]
    public void Email_ShouldFail_WhenEmpty()
        => _sut.TestValidate(new ForgotPasswordCommand(""))
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Theory]
    [InlineData("invalid")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void Email_ShouldFail_WhenInvalidFormat(string email)
        => _sut.TestValidate(new ForgotPasswordCommand(email))
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Validate_ShouldPass_WhenEmailValid()
        => _sut.TestValidate(new ForgotPasswordCommand("user@example.com"))
               .IsValid.Should().BeTrue();
}
