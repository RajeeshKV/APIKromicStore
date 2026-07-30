using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.ChangePassword;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _sut = new();

    [Fact]
    public void CurrentPassword_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { CurrentPassword = "" })
               .ShouldHaveValidationErrorFor(x => x.CurrentPassword);

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

    [Fact]
    public void NewPassword_ShouldFail_WhenSameAsCurrentPassword()
        => _sut.TestValidate(new ChangePasswordCommand("OldPass1!", "OldPass1!", "OldPass1!"))
               .ShouldHaveValidationErrorFor(x => x.NewPassword);

    [Fact]
    public void ConfirmPassword_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { ConfirmPassword = "" })
               .ShouldHaveValidationErrorFor(x => x.ConfirmPassword);

    [Fact]
    public void ConfirmPassword_ShouldFail_WhenMismatch()
        => _sut.TestValidate(Valid() with { ConfirmPassword = "WrongPass1!" })
               .ShouldHaveValidationErrorFor(x => x.ConfirmPassword);

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsValid()
        => _sut.TestValidate(Valid()).IsValid.Should().BeTrue();

    private static ChangePasswordCommand Valid() =>
        new("OldPass1!", "NewSecure1!", "NewSecure1!");
}
