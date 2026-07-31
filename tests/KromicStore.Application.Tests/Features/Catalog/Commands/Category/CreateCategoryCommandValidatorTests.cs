using FluentAssertions;
using FluentValidation.TestHelper;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Features.Catalog.Commands.CreateCategory;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.Category;

/// <summary>
/// Validator tests for CreateCategoryCommand.
/// </summary>
public sealed class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator;
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandValidatorTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _validator = new CreateCategoryCommandValidator(_categoryRepository);
    }

    #region Name Validation

    [Fact]
    public void Validate_NameRequired()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "",
            Description: null,
            Slug: "slug",
            DisplayOrder: 1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameCannotBeNull()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: null!,
            Description: null,
            Slug: "slug",
            DisplayOrder: 1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameMaxLength()
    {
        // Arrange
        var longName = string.Concat(Enumerable.Repeat("A", 300));
        var command = new CreateCategoryCommand(
            Name: longName,
            Description: null,
            Slug: "slug",
            DisplayOrder: 1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ValidName()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Electronics",
            Description: null,
            Slug: "electronics",
            DisplayOrder: 1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Display Order Validation

    [Fact]
    public void Validate_DisplayOrderNonNegative()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Test",
            Description: null,
            Slug: "test",
            DisplayOrder: -1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DisplayOrder);
    }

    [Fact]
    public void Validate_DisplayOrderZeroValid()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Test",
            Description: null,
            Slug: "test",
            DisplayOrder: 0,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.DisplayOrder);
    }

    #endregion

    #region Optional Fields

    [Fact]
    public void Validate_DescriptionOptional()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Test",
            Description: null,
            Slug: "test",
            DisplayOrder: 1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_SlugOptional()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Test",
            Description: null,
            Slug: null,
            DisplayOrder: 1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    #endregion

    #region Valid Scenarios

    [Fact]
    public void Validate_CompleteValid()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Electronics",
            Description: "Electronic devices and gadgets",
            Slug: "electronics",
            DisplayOrder: 1,
            IsVisible: true);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MinimalValid()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Books",
            Description: null,
            Slug: null,
            DisplayOrder: 0,
            IsVisible: false);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
