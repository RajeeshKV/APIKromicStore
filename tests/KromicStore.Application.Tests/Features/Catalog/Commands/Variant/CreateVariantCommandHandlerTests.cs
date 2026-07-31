using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.CreateVariant;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.Variant;

/// <summary>
/// Command tests for CreateVariantCommand.
/// </summary>
public sealed class CreateVariantCommandHandlerTests
{
    #region Happy Path

    [Fact]
    public void Command_CreatesVariant_WithValidData()
    {
        // Arrange & Act
        var command = new CreateVariantCommand(
            ProductId: Guid.NewGuid(),
            SkuSuffix: "RED-L",
            Name: "Red Large",
            PriceAdjustment: 10m,
            StockQuantity: 100);

        // Assert
        command.Should().NotBeNull();
        command.ProductId.Should().NotBe(Guid.Empty);
        command.Name.Should().Be("Red Large");
    }

    [Fact]
    public void Command_CreatesVariant_WithNegativePriceAdjustment()
    {
        // Arrange & Act
        var command = new CreateVariantCommand(
            ProductId: Guid.NewGuid(),
            SkuSuffix: "SALE",
            Name: "Sale Variant",
            PriceAdjustment: -5m,
            StockQuantity: 50);

        // Assert
        command.Should().NotBeNull();
        command.PriceAdjustment.Should().Be(-5m);
    }

    [Fact]
    public void Command_CreatesVariant_ZeroStock()
    {
        // Arrange & Act
        var command = new CreateVariantCommand(
            ProductId: Guid.NewGuid(),
            SkuSuffix: "OUT",
            Name: "Out of Stock",
            PriceAdjustment: 0m,
            StockQuantity: 0);

        // Assert
        command.Should().NotBeNull();
        command.StockQuantity.Should().Be(0);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Command_WithHighStock()
    {
        // Arrange & Act
        var command = new CreateVariantCommand(
            ProductId: Guid.NewGuid(),
            SkuSuffix: "BIG",
            Name: "High Stock",
            PriceAdjustment: 0m,
            StockQuantity: 10000);

        // Assert
        command.Should().NotBeNull();
        command.StockQuantity.Should().Be(10000);
    }

    [Fact]
    public void Command_WithHighPriceAdjustment()
    {
        // Arrange & Act
        var command = new CreateVariantCommand(
            ProductId: Guid.NewGuid(),
            SkuSuffix: "PREMIUM",
            Name: "Premium",
            PriceAdjustment: 500m,
            StockQuantity: 5);

        // Assert
        command.Should().NotBeNull();
        command.PriceAdjustment.Should().Be(500m);
    }

    #endregion
}
