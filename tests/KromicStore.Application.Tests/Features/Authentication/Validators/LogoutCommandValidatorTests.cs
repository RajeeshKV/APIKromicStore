using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.Logout;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _sut = new();

    [Fact]
    public void RefreshToken_ShouldFail_WhenEmpty()
        => _sut.TestValidate(new LogoutCommand(""))
               .ShouldHaveValidationErrorFor(x => x.RefreshToken);

    [Fact]
    public void RefreshToken_ShouldFail_WhenWhitespace()
        => _sut.TestValidate(new LogoutCommand("   "))
               .ShouldHaveValidationErrorFor(x => x.RefreshToken);

    [Fact]
    public void Validate_ShouldPass_WhenTokenProvided()
        => _sut.TestValidate(new LogoutCommand("some-token"))
               .IsValid.Should().BeTrue();
}
