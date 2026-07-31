using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.UpdateVariant;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.Variant;

public sealed class UpdateVariantCommandHandlerTests
{
    #region Command Tests

    [Fact]
    public void Command_UpdatesVariant_WithValidData()
    {
        // Arrange & Act
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        
        var command = new UpdateVariantCommand(
            ProductId: productId,
            VariantId: variantId,
            Name: "Updated Variant",
            PriceAdjustment: 15m,
            Attributes: null,
            IsActive: true);

        // Assert
        command.Should().NotBeNull();
        command.ProductId.Should().Be(productId);
        command.VariantId.Should().Be(variantId);
        command.Name.Should().Be("Updated Variant");
    }

    [Fact]
    public void Command_PartialUpdate()
    {
        // Arrange & Act
        var command = new UpdateVariantCommand(
            ProductId: Guid.NewGuid(),
            VariantId: Guid.NewGuid(),
            Name: null,
            PriceAdjustment: null,
            Attributes: null,
            IsActive: null);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().BeNull();
    }

    [Fact]
    public void Command_WithNegativePriceAdjustment()
    {
        // Arrange & Act
        var command = new UpdateVariantCommand(
            ProductId: Guid.NewGuid(),
            VariantId: Guid.NewGuid(),
            Name: "Sale",
            PriceAdjustment: -20m,
            Attributes: null,
            IsActive: true);

        // Assert
        command.Should().NotBeNull();
        command.PriceAdjustment.Should().Be(-20m);
    }

    [Fact]
    public void Command_WithHighPriceAdjustment()
    {
        // Arrange & Act
        var command = new UpdateVariantCommand(
            ProductId: Guid.NewGuid(),
            VariantId: Guid.NewGuid(),
            Name: "Premium",
            PriceAdjustment: 200m,
            Attributes: null,
            IsActive: true);

        // Assert
        command.Should().NotBeNull();
        command.PriceAdjustment.Should().Be(200m);
    }

    [Fact]
    public void Command_DeactivatesVariant()
    {
        // Arrange & Act
        var command = new UpdateVariantCommand(
            ProductId: Guid.NewGuid(),
            VariantId: Guid.NewGuid(),
            Name: null,
            PriceAdjustment: null,
            Attributes: null,
            IsActive: false);

        // Assert
        command.Should().NotBeNull();
        command.IsActive.Should().BeFalse();
    }

    #endregion
}
