using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.Login;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    [Fact]
    public void Email_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { Email = "" })
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@missing.com")]
    [InlineData("missing@")]
    public void Email_ShouldFail_WhenInvalidFormat(string email)
        => _sut.TestValidate(Valid() with { Email = email })
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Password_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { Password = "" })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Validate_ShouldPass_WhenValid()
        => _sut.TestValidate(Valid()).IsValid.Should().BeTrue();

    private static LoginCommand Valid() =>
        new("alice@example.com", "anypassword", null, null);
}
