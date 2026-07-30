using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.RefreshToken;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _sut = new();

    [Fact]
    public void Token_ShouldFail_WhenEmpty()
        => _sut.TestValidate(new RefreshTokenCommand("", null, null))
               .ShouldHaveValidationErrorFor(x => x.Token);

    [Fact]
    public void Token_ShouldFail_WhenWhitespace()
        => _sut.TestValidate(new RefreshTokenCommand("   ", null, null))
               .ShouldHaveValidationErrorFor(x => x.Token);

    [Fact]
    public void Validate_ShouldPass_WhenTokenProvided()
        => _sut.TestValidate(new RefreshTokenCommand("valid-token", null, null))
               .IsValid.Should().BeTrue();
}
