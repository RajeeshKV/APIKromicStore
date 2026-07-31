using FluentAssertions;
using KromicStore.Domain.StoreOperations.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.StoreOperations.Entities;

public class ReturnInspectionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _returnRequestId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidData_CreatesInspection()
    {
        // Act
        var inspection = ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            "Items in perfect condition",
            true,
            50m,
            0m,
            "inspector@test.com");
        
        // Assert
        inspection.Should().NotBeNull();
        inspection.TenantId.Should().Be(_tenantId);
        inspection.ReturnRequestId.Should().Be(_returnRequestId);
        inspection.Result.Should().Be(InspectionResult.Acceptable);
        inspection.InspectorNotes.Should().Be("Items in perfect condition");
        inspection.IsRestockable.Should().BeTrue();
        inspection.RestockableValue.Should().Be(50m);
        inspection.WasteValue.Should().Be(0m);
        inspection.InspectedBy.Should().Be("inspector@test.com");
    }
    
    [Fact]
    public void Create_WithEmptyReturnRequestId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnInspection.Create(
            _tenantId,
            Guid.Empty,
            InspectionResult.Acceptable,
            "Notes",
            true,
            50m,
            0m,
            "inspector@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Return request ID is required*");
    }
    
    [Fact]
    public void Create_WithNullInspectorNotes_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            null!,
            true,
            50m,
            0m,
            "inspector@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Inspector notes are required*");
    }
    
    [Fact]
    public void Create_WithNegativeRestockableValue_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            "Notes",
            true,
            -10m,
            0m,
            "inspector@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Restockable value cannot be negative*");
    }
    
    [Fact]
    public void Create_WithNegativeWasteValue_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            "Notes",
            true,
            50m,
            -10m,
            "inspector@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Waste value cannot be negative*");
    }
    
    [Fact]
    public void Create_WithBothValuesZero_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            "Notes",
            false,
            0m,
            0m,
            "inspector@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Either restockable value or waste value must be greater than zero*");
    }
    
    [Theory]
    [InlineData(InspectionResult.Acceptable)]
    [InlineData(InspectionResult.MinorDefects)]
    [InlineData(InspectionResult.MajorDefects)]
    [InlineData(InspectionResult.Unopened)]
    [InlineData(InspectionResult.Wrong)]
    public void Create_WithAllResults_Creates(InspectionResult result)
    {
        // Act
        var inspection = ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            result,
            "Test notes",
            true,
            50m,
            0m,
            "inspector@test.com");
        
        // Assert
        inspection.Result.Should().Be(result);
    }
    
    [Fact]
    public void GetTotalInspectionValue_ReturnsSumOfValues()
    {
        // Arrange
        var inspection = ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.MinorDefects,
            "Minor damage to box",
            true,
            40m,
            10m,
            "inspector@test.com");
        
        // Act
        var totalValue = inspection.GetTotalInspectionValue();
        
        // Assert
        totalValue.Should().Be(50m);
    }
    
    [Theory]
    [InlineData(InspectionResult.Acceptable, true)]
    [InlineData(InspectionResult.Unopened, true)]
    [InlineData(InspectionResult.MinorDefects, true)]
    [InlineData(InspectionResult.MajorDefects, false)]
    [InlineData(InspectionResult.Wrong, false)]
    public void CanBeRestocked_ReturnsCorrectResult(InspectionResult result, bool expectedRestockable)
    {
        // Arrange
        var inspection = ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            result,
            "Test notes",
            true,
            50m,
            0m,
            "inspector@test.com");
        
        // Act
        var canRestock = inspection.CanBeRestocked();
        
        // Assert
        canRestock.Should().Be(expectedRestockable);
    }
    
    [Fact]
    public void UpdateInspection_WithValidData_Updates()
    {
        // Arrange
        var inspection = ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            "Perfect condition",
            true,
            50m,
            0m,
            "inspector@test.com");
        
        // Act
        inspection.UpdateInspection(
            InspectionResult.MinorDefects,
            "Minor scratches found",
            true,
            35m,
            15m);
        
        // Assert
        inspection.Result.Should().Be(InspectionResult.MinorDefects);
        inspection.InspectorNotes.Should().Be("Minor scratches found");
        inspection.RestockableValue.Should().Be(35m);
        inspection.WasteValue.Should().Be(15m);
    }
    
    [Fact]
    public void UpdateInspection_WithNullNotes_ThrowsArgumentException()
    {
        // Arrange
        var inspection = ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            "Perfect condition",
            true,
            50m,
            0m,
            "inspector@test.com");
        
        // Act & Assert
        var act = () => inspection.UpdateInspection(
            InspectionResult.MinorDefects,
            null!,
            true,
            35m,
            15m);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Notes are required*");
    }
    
    [Fact]
    public void UpdateInspection_WithBothValuesZero_ThrowsArgumentException()
    {
        // Arrange
        var inspection = ReturnInspection.Create(
            _tenantId,
            _returnRequestId,
            InspectionResult.Acceptable,
            "Perfect condition",
            true,
            50m,
            0m,
            "inspector@test.com");
        
        // Act & Assert
        var act = () => inspection.UpdateInspection(
            InspectionResult.MajorDefects,
            "Cannot be restocked",
            false,
            0m,
            0m);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Either restockable value or waste value must be greater than zero*");
    }
}
