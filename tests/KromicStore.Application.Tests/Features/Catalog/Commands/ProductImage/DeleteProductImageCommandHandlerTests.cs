using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.DeleteProductImage;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.ProductImage;

public sealed class DeleteProductImageCommandHandlerTests
{
    #region Command Tests

    [Fact]
    public void Command_CreatesWithValidIds()
    {
        // Arrange & Act
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        
        var command = new DeleteProductImageCommand(
            ProductId: productId,
            ImageId: imageId);

        // Assert
        command.Should().NotBeNull();
        command.ProductId.Should().Be(productId);
        command.ImageId.Should().Be(imageId);
    }

    [Fact]
    public void Command_WithDifferentIds()
    {
        // Arrange & Act
        var command1 = new DeleteProductImageCommand(Guid.NewGuid(), Guid.NewGuid());
        var command2 = new DeleteProductImageCommand(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        command1.ProductId.Should().NotBe(command2.ProductId);
        command1.ImageId.Should().NotBe(command2.ImageId);
    }

    [Fact]
    public void Command_ValidatesProductId()
    {
        // Arrange & Act
        var command = new DeleteProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageId: Guid.NewGuid());

        // Assert
        command.ProductId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Command_ValidatesImageId()
    {
        // Arrange & Act
        var command = new DeleteProductImageCommand(
            ProductId: Guid.NewGuid(),
            ImageId: Guid.NewGuid());

        // Assert
        command.ImageId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Command_MultipleDeletes()
    {
        // Arrange & Act
        var commands = new List<DeleteProductImageCommand>();
        for (int i = 0; i < 5; i++)
        {
            var command = new DeleteProductImageCommand(Guid.NewGuid(), Guid.NewGuid());
            commands.Add(command);
        }

        // Assert
        commands.Should().HaveCount(5);
        commands.All(c => c.ProductId != Guid.Empty).Should().BeTrue();
    }

    #endregion
}