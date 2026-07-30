using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.ResetPassword;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _sut = new();

    // ── Token ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Token_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { Token = "" })
               .ShouldHaveValidationErrorFor(x => x.Token);

    // ── NewPassword ───────────────────────────────────────────────────────────

    [Fact]
    public void NewPassword_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { NewPassword = "" })
               .ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void NewPassword_ShouldFail_WhenTooShort()
        => _sut.TestValidate(Valid() with { NewPassword = "Ab1!" })
               .ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void NewPassword_ShouldFail_WhenNoUppercase()
        => _sut.TestValidate(Valid() with { NewPassword = "password1!" })
               .ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void NewPassword_ShouldFail_WhenNoLowercase()
        => _sut.TestValidate(Valid() with { NewPassword = "PASSWORD1!" })
               .ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void NewPassword_ShouldFail_WhenNoDigit()
        => _sut.TestValidate(Valid() with { NewPassword = "Password!" })
               .ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void NewPassword_ShouldFail_WhenNoSpecialChar()
        => _sut.TestValidate(Valid() with { NewPassword = "Password1" })
               .ShouldHaveValidationErrorFor(x => x.NewPassword);

    // ── ConfirmPassword ───────────────────────────────────────────────────────

    [Fact]
    public void ConfirmPassword_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { ConfirmPassword = "" })
               .ShouldHaveValidationErrorFor(x => x.ConfirmPassword);

    [Fact]
    public void ConfirmPassword_ShouldFail_WhenMismatch()
        => _sut.TestValidate(Valid() with { ConfirmPassword = "DifferentPass1!" })
               .ShouldHaveValidationErrorFor(x => x.ConfirmPassword);

    // ── Full valid ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsValid()
        => _sut.TestValidate(Valid()).IsValid.Should().BeTrue();

    private static ResetPasswordCommand Valid() =>
        new("valid-token", "NewSecure1!", "NewSecure1!");
}
