using FluentAssertions;
using KromicStore.Domain.Catalog.Entities;

namespace KromicStore.Domain.Tests.Catalog.Entities;

/// <summary>
/// Domain tests for Category aggregate root.
/// Verifies category creation, hierarchy, soft delete, and slug generation.
/// </summary>
public sealed class CategoryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    #region Creation Tests

    [Fact]
    public void Create_WithRequiredFields_CreatesCategorySuccessfully()
    {
        // Act
        var category = Category.Create(
            tenantId: _tenantId,
            name: "Electronics");

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().NotBeEmpty();
        category.TenantId.Should().Be(_tenantId);
        category.Name.Should().Be("Electronics");
        category.Status.Should().Be(CategoryStatus.Active);
        category.IsVisible.Should().BeTrue();
        category.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void Create_WithOptionalFields_StoresAllFields()
    {
        // Act
        var category = Category.Create(
            tenantId: _tenantId,
            name: "Computers",
            customSlug: "computers-section",
            description: "All computer products",
            parentCategoryId: null,
            displayOrder: 5,
            isVisible: false,
            status: CategoryStatus.Archived,
            imageUrl: "https://example.com/image.jpg");

        // Assert
        category.Slug.Should().Be("computers-section");
        category.Description.Should().Be("All computer products");
        category.DisplayOrder.Should().Be(5);
        category.IsVisible.Should().BeFalse();
        category.Status.Should().Be(CategoryStatus.Archived);
        category.ImageUrl.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public void Create_WithoutCustomSlug_GeneratesSlugFromName()
    {
        // Act
        var category = Category.Create(
            tenantId: _tenantId,
            name: "Mobile Phones");

        // Assert
        category.Slug.Should().NotBeNullOrEmpty();
        category.Slug.Should().Contain("mobile");
    }

    [Fact]
    public void Create_WithParentCategory_CreatesChildCategory()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var category = Category.Create(
            tenantId: _tenantId,
            name: "Laptops",
            parentCategoryId: parentId);

        // Assert
        category.ParentCategoryId.Should().Be(parentId);
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        // Act & Assert
        var act = () => Category.Create(
            tenantId: _tenantId,
            name: "");

        act.Should().Throw<ArgumentException>().WithMessage("*Name*");
    }

    [Fact]
    public void Create_NameTooLong_ThrowsException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act & Assert
        var act = () => Category.Create(
            tenantId: _tenantId,
            name: longName);

        act.Should().Throw<ArgumentException>().WithMessage("*Name*");
    }

    #endregion

    #region Hierarchy Tests

    [Fact]
    public void Update_WithParentCategory_CreatesHierarchy()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Subcategory");
        var parentId = Guid.NewGuid();

        // Act
        category.Update(parentCategoryId: parentId);

        // Assert
        category.ParentCategoryId.Should().Be(parentId);
    }

    [Fact]
    public void Update_CircularReference_ThrowsException()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");

        // Act & Assert
        var act = () => category.Update(parentCategoryId: category.Id);
        act.Should().Throw<InvalidOperationException>().WithMessage("*parent*");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithNewName_ChangesName()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Original");

        // Act
        category.Update(name: "Updated");

        // Assert
        category.Name.Should().Be("Updated");
    }

    [Fact]
    public void Update_WithNewSlug_ChangesSlug()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");

        // Act
        category.Update(customSlug: "new-slug");

        // Assert
        category.Slug.Should().Be("new-slug");
    }

    [Fact]
    public void Update_WithNewDescription_ChangesDescription()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");

        // Act
        category.Update(description: "New description");

        // Assert
        category.Description.Should().Be("New description");
    }

    [Fact]
    public void Update_VisibilityToggle_ChangesVisibility()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category", isVisible: true);

        // Act
        category.Update(isVisible: false);

        // Assert
        category.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void Update_DisplayOrder_ChangesOrder()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category", displayOrder: 0);

        // Act
        category.Update(displayOrder: 10);

        // Assert
        category.DisplayOrder.Should().Be(10);
    }

    [Fact]
    public void Update_NullFields_DoesNotChangeFields()
    {
        // Arrange
        var original = Category.Create(
            _tenantId,
            "Original Name",
            isVisible: true,
            displayOrder: 5);

        // Act
        original.Update(
            name: null,
            isVisible: null,
            displayOrder: null);

        // Assert
        original.Name.Should().Be("Original Name");
        original.IsVisible.Should().BeTrue();
        original.DisplayOrder.Should().Be(5);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void Archive_Category_ChangesStatusAndVisibility()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category", isVisible: true);

        // Act
        category.Archive();

        // Assert
        category.Status.Should().Be(CategoryStatus.Archived);
        category.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void Unarchive_Category_ReversesArchive()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");
        category.Archive();

        // Act
        category.Unarchive();

        // Assert
        category.Status.Should().Be(CategoryStatus.Active);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDelete_Category_SetsDeleteFlags()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");
        var now = DateTime.UtcNow;

        // Act
        category.SoftDelete(now, "admin-user");

        // Assert
        category.IsDeleted.Should().BeTrue();
        category.DeletedOnUtc.Should().Be(now);
        category.DeletedBy.Should().Be("admin-user");
    }

    [Fact]
    public void Restore_DeletedCategory_ClearsDeleteFlags()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");
        category.SoftDelete(DateTime.UtcNow, "admin");

        // Act
        category.Restore(DateTime.UtcNow, "admin");

        // Assert
        category.IsDeleted.Should().BeFalse();
        category.DeletedOnUtc.Should().BeNull();
        category.DeletedBy.Should().BeNull();
    }

    #endregion

    #region Audit Field Tests

    [Fact]
    public void MarkCreated_SetsAuditFields()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");
        var now = DateTime.UtcNow;

        // Act
        category.MarkCreated(now, "user-123");

        // Assert
        category.CreatedOnUtc.Should().Be(now);
        category.CreatedBy.Should().Be("user-123");
        category.ModifiedOnUtc.Should().Be(now);
        category.ModifiedBy.Should().Be("user-123");
    }

    [Fact]
    public void MarkModified_UpdatesModificationFields()
    {
        // Arrange
        var category = Category.Create(_tenantId, "Category");
        var createdTime = DateTime.UtcNow;
        category.MarkCreated(createdTime, "user-1");
        
        var modifiedTime = createdTime.AddMinutes(5);

        // Act
        category.MarkModified(modifiedTime, "user-2");

        // Assert
        category.CreatedOnUtc.Should().Be(createdTime);
        category.CreatedBy.Should().Be("user-1");
        category.ModifiedOnUtc.Should().Be(modifiedTime);
        category.ModifiedBy.Should().Be("user-2");
    }

    #endregion
}
