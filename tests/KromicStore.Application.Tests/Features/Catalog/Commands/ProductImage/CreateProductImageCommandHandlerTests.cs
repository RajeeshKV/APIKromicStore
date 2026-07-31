using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.CreateProductImage;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.ProductImage;

/// <summary>
/// Command tests for CreateProductImageCommand.
/// </summary>
public sealed class CreateProductImageCommandHandlerTests
{
    #region Happy Path Tests

    [Fact]
    public void Command_CreatesWithValidUrl()
    {
        // Arrange & Act
        var command = new CreateProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageUrl: "https://example.com/image1.jpg",
            AltText: "Product Image",
            IsPrimary: true);

        // Assert
        command.Should().NotBeNull();
        command.ProductId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Command_CreatesWithoutAltText()
    {
        // Arrange & Act
        var command = new CreateProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageUrl: "https://example.com/image2.jpg",
            AltText: null,
            IsPrimary: false);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Command_CreatesSecondaryImage()
    {
        // Arrange & Act
        var command = new CreateProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageUrl: "https://example.com/secondary.jpg",
            AltText: "Secondary",
            IsPrimary: false);

        // Assert
        command.Should().NotBeNull();
        command.IsPrimary.Should().BeFalse();
    }

    #endregion

    #region Multiple Images

    [Fact]
    public void Command_CreatesMultipleImages()
    {
        // Arrange & Act
        var command1 = new CreateProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageUrl: "https://example.com/image1.jpg",
            AltText: "Image 1",
            IsPrimary: true);

        var command2 = new CreateProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageUrl: "https://example.com/image2.jpg",
            AltText: "Image 2",
            IsPrimary: false);

        // Assert
        command1.ProductId.Should().NotBe(Guid.Empty);
        command2.ProductId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Command_WithDifferentPrimaryStatus()
    {
        // Arrange & Act
        var command1 = new CreateProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageUrl: "https://example.com/primary.jpg",
            AltText: "Primary",
            IsPrimary: true);

        var command2 = new CreateProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageUrl: "https://example.com/secondary.jpg",
            AltText: "Secondary",
            IsPrimary: false);

        // Assert
        command1.IsPrimary.Should().BeTrue();
        command2.IsPrimary.Should().BeFalse();
    }

    #endregion
}
