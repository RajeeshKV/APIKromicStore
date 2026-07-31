using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.UpdateCollection;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.Collection;

public sealed class UpdateCollectionCommandHandlerTests
{
    #region Command Tests

    [Fact]
    public void Command_UpdatesCollection_WithValidData()
    {
        // Arrange & Act
        var collectionId = Guid.NewGuid();
        
        var command = new UpdateCollectionCommand(
            CollectionId: collectionId,
            Name: "Updated Name",
            Description: "Updated desc",
            DisplayOrder: 5,
            Status: "Inactive");

        // Assert
        command.Should().NotBeNull();
        command.CollectionId.Should().Be(collectionId);
        command.Name.Should().Be("Updated Name");
        command.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Command_PartialUpdate()
    {
        // Arrange & Act
        var command = new UpdateCollectionCommand(
            CollectionId: Guid.NewGuid(),
            Name: null,
            Description: null,
            DisplayOrder: null,
            Status: null);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().BeNull();
    }

    [Fact]
    public void Command_WithNewStatus()
    {
        // Arrange & Act
        var command = new UpdateCollectionCommand(
            CollectionId: Guid.NewGuid(),
            Name: "Test",
            Description: null,
            DisplayOrder: 1,
            Status: "Inactive");

        // Assert
        command.Should().NotBeNull();
        command.Status.Should().Be("Inactive");
    }

    [Fact]
    public void Command_WithHighDisplayOrder()
    {
        // Arrange & Act
        var command = new UpdateCollectionCommand(
            CollectionId: Guid.NewGuid(),
            Name: "Test",
            Description: null,
            DisplayOrder: 999,
            Status: "Active");

        // Assert
        command.Should().NotBeNull();
        command.DisplayOrder.Should().Be(999);
    }

    [Fact]
    public void Command_WithLongDescription()
    {
        // Arrange & Act
        var longDesc = string.Concat(Enumerable.Repeat("Description text. ", 50));
        var command = new UpdateCollectionCommand(
            CollectionId: Guid.NewGuid(),
            Name: "Test",
            Description: longDesc,
            DisplayOrder: 1,
            Status: "Active");

        // Assert
        command.Should().NotBeNull();
        command.Description.Should().Contain("Description text");
    }

    [Fact]
    public void Command_MultipleUpdates()
    {
        // Arrange & Act
        var commands = new List<UpdateCollectionCommand>();
        for (int i = 0; i < 5; i++)
        {
            var command = new UpdateCollectionCommand(
                CollectionId: Guid.NewGuid(),
                Name: $"Collection {i}",
                Description: null,
                DisplayOrder: i,
                Status: "Active");
            commands.Add(command);
        }

        // Assert
        commands.Should().HaveCount(5);
        commands.All(c => c.CollectionId != Guid.Empty).Should().BeTrue();
    }

    #endregion
}
