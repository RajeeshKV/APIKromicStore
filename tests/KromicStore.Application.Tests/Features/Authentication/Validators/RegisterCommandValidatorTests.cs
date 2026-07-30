using FluentValidation.TestHelper;
using KromicStore.Application.Features.Authentication.Commands.Register;

namespace KromicStore.Application.Tests.Features.Authentication.Validators;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _sut = new();

    // ── FirstName ─────────────────────────────────────────────────────────────

    [Fact]
    public void FirstName_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { FirstName = "" })
               .ShouldHaveValidationErrorFor(x => x.FirstName);

    [Fact]
    public void FirstName_ShouldFail_WhenExceeds100Chars()
        => _sut.TestValidate(Valid() with { FirstName = new string('A', 101) })
               .ShouldHaveValidationErrorFor(x => x.FirstName);

    [Fact]
    public void FirstName_ShouldPass_WhenExactly100Chars()
        => _sut.TestValidate(Valid() with { FirstName = new string('A', 100) })
               .ShouldNotHaveValidationErrorFor(x => x.FirstName);

    // ── LastName ──────────────────────────────────────────────────────────────

    [Fact]
    public void LastName_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { LastName = "" })
               .ShouldHaveValidationErrorFor(x => x.LastName);

    [Fact]
    public void LastName_ShouldFail_WhenExceeds100Chars()
        => _sut.TestValidate(Valid() with { LastName = new string('B', 101) })
               .ShouldHaveValidationErrorFor(x => x.LastName);

    // ── Email ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Email_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { Email = "" })
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    [InlineData("spaces in@email.com")]
    public void Email_ShouldFail_WhenInvalidFormat(string email)
        => _sut.TestValidate(Valid() with { Email = email })
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Email_ShouldFail_WhenExceeds256Chars()
    {
        var email = new string('a', 247) + "@test.com"; // 256 + 1
        _sut.TestValidate(Valid() with { Email = email })
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_ShouldPass_WhenValid()
        => _sut.TestValidate(Valid())
               .ShouldNotHaveValidationErrorFor(x => x.Email);

    // ── Password ──────────────────────────────────────────────────────────────

    [Fact]
    public void Password_ShouldFail_WhenEmpty()
        => _sut.TestValidate(Valid() with { Password = "" })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_ShouldFail_WhenTooShort()
        => _sut.TestValidate(Valid() with { Password = "Ab1!" })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_ShouldFail_WhenNoUppercase()
        => _sut.TestValidate(Valid() with { Password = "password1!" })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_ShouldFail_WhenNoLowercase()
        => _sut.TestValidate(Valid() with { Password = "PASSWORD1!" })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_ShouldFail_WhenNoDigit()
        => _sut.TestValidate(Valid() with { Password = "Password!" })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_ShouldFail_WhenNoSpecialChar()
        => _sut.TestValidate(Valid() with { Password = "Password1" })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_ShouldFail_WhenExceeds128Chars()
        => _sut.TestValidate(Valid() with { Password = "Password1!" + new string('x', 119) })
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_ShouldPass_WhenMeetsAllRequirements()
        => _sut.TestValidate(Valid())
               .ShouldNotHaveValidationErrorFor(x => x.Password);

    // ── Full valid command ────────────────────────────────────────────────────

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsValid()
        => _sut.TestValidate(Valid()).IsValid.Should().BeTrue();

    // ── Helper ────────────────────────────────────────────────────────────────

    private static RegisterCommand Valid() => new(
        FirstName:  "Alice",
        LastName:   "Smith",
        Email:      "alice@example.com",
        Password:   "SecurePass1!",
        DeviceName: null,
        IpAddress:  null);
}
