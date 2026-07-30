# Phase 4 - Test Implementation Strategy

**Date:** July 30, 2026  
**Purpose:** Define how to implement tests based on the test matrix  
**Status:** READY FOR IMPLEMENTATION

---

## Implementation Phases

### Phase 1: Domain Entity Tests

**Location:** `tests/KromicStore.Domain.Tests/Catalog/`

**Structure:**
```
Catalog/
├── Entities/
│   ├── ProductTests.cs (60+ tests)
│   ├── CategoryTests.cs (30+ tests)
│   ├── ProductVariantTests.cs (25+ tests)
│   ├── ProductImageTests.cs (20+ tests)
│   └── ProductInventoryTests.cs (15+ tests)
├── ValueObjects/
│   ├── SKUTests.cs (10+ tests)
│   └── SlugTests.cs (10+ tests)
└── Fixtures/
    └── CatalogTestFixtures.cs
```

**Test Framework:** XUnit + Fluent Assertions

**Example Test:**
```csharp
public class ProductTests
{
    [Fact]
    public void Create_ValidProduct_CreatesSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        // Act
        var product = Product.Create(
            tenantId: tenantId,
            categoryId: categoryId,
            sku: "PROD-001",
            name: "Test Product"
        );
        
        // Assert
        product.Should().NotBeNull();
        product.Sku.Should().Be("PROD-001");
        product.Status.Should().Be(ProductStatus.Draft);
        product.Inventory.Should().NotBeNull();
    }

    [Fact]
    public void Create_InvalidSKU_ThrowsException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Product.Create(
                tenantId: tenantId,
                categoryId: categoryId,
                sku: "invalid-sku-lowercase",  // Invalid
                name: "Test Product"
            )
        );
        
        ex.Message.Should().Contain("SKU");
    }

    [Fact]
    public void SoftDelete_SetsFlags()
    {
        // Arrange
        var product = Product.Create(/* ... */);
        var now = DateTime.UtcNow;
        
        // Act
        product.SoftDelete(now, "admin");
        
        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedOnUtc.Should().Be(now);
        product.DeletedBy.Should().Be("admin");
    }

    [Fact]
    public void Restore_ClearsDeleteFlags()
    {
        // Arrange
        var product = Product.Create(/* ... */);
        product.SoftDelete(DateTime.UtcNow, "admin");
        
        // Act
        product.Restore();
        
        // Assert
        product.IsDeleted.Should().BeFalse();
        product.DeletedOnUtc.Should().BeNull();
        product.DeletedBy.Should().BeNull();
    }
}
```

---

### Phase 2: Command Handler Tests

**Location:** `tests/KromicStore.Application.Tests/Features/Catalog/Commands/`

**Structure:**
```
Catalog/Commands/
├── CreateProduct/
│   ├── CreateProductCommandHandlerTests.cs (8 tests)
│   └── CreateProductCommandValidatorTests.cs (12 tests)
├── UpdateProduct/
│   ├── UpdateProductCommandHandlerTests.cs (8 tests)
│   └── UpdateProductCommandValidatorTests.cs (10 tests)
├── DeleteProduct/
│   ├── DeleteProductCommandHandlerTests.cs (3 tests)
│   └── DeleteProductCommandValidatorTests.cs (3 tests)
├── RestoreProduct/
│   ├── RestoreProductCommandHandlerTests.cs (3 tests)
│   └── RestoreProductCommandValidatorTests.cs (3 tests)
├── DuplicateProduct/
│   ├── DuplicateProductCommandHandlerTests.cs (6 tests)
│   └── DuplicateProductCommandValidatorTests.cs (4 tests)
├── CreateProductImage/
│   ├── CreateProductImageCommandHandlerTests.cs (4 tests)
│   └── CreateProductImageCommandValidatorTests.cs (5 tests)
├── DeleteProductImage/
│   ├── DeleteProductImageCommandHandlerTests.cs (3 tests)
│   └── DeleteProductImageCommandValidatorTests.cs (3 tests)
├── CreateVariant/
│   ├── CreateVariantCommandHandlerTests.cs (5 tests)
│   └── CreateVariantCommandValidatorTests.cs (6 tests)
├── UpdateVariant/
│   ├── UpdateVariantCommandHandlerTests.cs (4 tests)
│   └── UpdateVariantCommandValidatorTests.cs (5 tests)
├── DeleteVariant/
│   ├── DeleteVariantCommandHandlerTests.cs (3 tests)
│   └── DeleteVariantCommandValidatorTests.cs (3 tests)
├── AdjustInventory/
│   ├── AdjustInventoryCommandHandlerTests.cs (5 tests)
│   └── AdjustInventoryCommandValidatorTests.cs (5 tests)
├── CreateCategory/
│   ├── CreateCategoryCommandHandlerTests.cs (6 tests)
│   └── CreateCategoryCommandValidatorTests.cs (8 tests)
├── UpdateCategory/
│   ├── UpdateCategoryCommandHandlerTests.cs (5 tests)
│   └── UpdateCategoryCommandValidatorTests.cs (7 tests)
├── DeleteCategory/
│   ├── DeleteCategoryCommandHandlerTests.cs (3 tests)
│   └── DeleteCategoryCommandValidatorTests.cs (3 tests)
├── RestoreCategory/
│   ├── RestoreCategoryCommandHandlerTests.cs (3 tests)
│   └── RestoreCategoryCommandValidatorTests.cs (3 tests)
├── CreateCollection/
│   ├── CreateCollectionCommandHandlerTests.cs (5 tests)
│   └── CreateCollectionCommandValidatorTests.cs (6 tests)
├── UpdateCollection/
│   ├── UpdateCollectionCommandHandlerTests.cs (4 tests)
│   └── UpdateCollectionCommandValidatorTests.cs (5 tests)
└── DeleteCollection/
    ├── DeleteCollectionCommandHandlerTests.cs (3 tests)
    └── DeleteCollectionCommandValidatorTests.cs (3 tests)
```

**Total Command Tests:** ~180 tests

**Setup Pattern:**
```csharp
public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ICurrentUserService> _userServiceMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _tenantContextMock = new Mock<ITenantContext>();
        _userServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _dbContextMock.Object,
            new Mock<ILogger<CreateProductCommandHandler>>().Object,
            _tenantContextMock.Object,
            _userServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesProduct()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            categoryId: categoryId,
            name: "Test Product",
            sku: "TEST-001"
        );
        
        var category = Category.Create(tenantId, "Category");
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        
        _productRepositoryMock
            .Setup(x => x.SkuExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _tenantContextMock.Setup(x => x.TenantId).Returns(tenantId);
        _userServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().NotBeEmpty();
        result.Sku.Should().Be("TEST-001");
        _productRepositoryMock.Verify(x => x.Add(It.IsAny<Product>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsException()
    {
        // Arrange
        var command = new CreateProductCommand(
            categoryId: Guid.NewGuid(),
            name: "Test",
            sku: "DUPLICATE"
        );
        
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Category.Create(Guid.NewGuid(), "Category"));
        
        _productRepositoryMock
            .Setup(x => x.SkuExistsAsync("DUPLICATE", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            categoryId: categoryId,
            name: "Test",
            sku: "TEST"
        );
        
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_TenantContextNull_ThrowsException()
    {
        // Arrange
        var command = new CreateProductCommand(
            categoryId: Guid.NewGuid(),
            name: "Test",
            sku: "TEST"
        );
        
        _tenantContextMock.Setup(x => x.TenantId).Returns((Guid?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
    }
}
```

---

### Phase 3: Query Handler Tests

**Location:** `tests/KromicStore.Application.Tests/Features/Catalog/Queries/`

**Structure:**
```
Catalog/Queries/
├── GetProducts/
│   └── GetProductsQueryHandlerTests.cs (8 tests)
├── GetProductById/
│   └── GetProductByIdQueryHandlerTests.cs (5 tests)
├── GetCategories/
│   └── GetCategoriesQueryHandlerTests.cs (5 tests)
├── GetCategoryById/
│   └── GetCategoryByIdQueryHandlerTests.cs (4 tests)
├── GetVariants/
│   └── GetVariantsQueryHandlerTests.cs (4 tests)
├── GetCollections/
│   └── GetCollectionsQueryHandlerTests.cs (4 tests)
├── GetCollectionById/
│   └── GetCollectionByIdQueryHandlerTests.cs (3 tests)
├── GetInventory/
│   └── GetInventoryQueryHandlerTests.cs (4 tests)
├── SearchProducts/
│   └── SearchProductsQueryHandlerTests.cs (5 tests)
├── SearchCategories/
│   └── SearchCategoriesQueryHandlerTests.cs (4 tests)
└── GetProductImages/
    └── GetProductImagesQueryHandlerTests.cs (4 tests)
```

**Test Pattern:**
```csharp
public class GetProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_AllProducts_ReturnsAllActiveProducts()
    {
        // Setup with in-memory database
        // Create test products
        // Query with no filters
        // Assert all products returned
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Create 25 products
        // Query with Skip=10, Take=10
        // Assert 10 products returned, correct ones
    }

    [Fact]
    public async Task Handle_FilterByCategory_ReturnsOnlyCategory()
    {
        // Create products in multiple categories
        // Query with specific CategoryId
        // Assert only matching category products
    }

    [Fact]
    public async Task Handle_ExcludesDeletedProducts_NotReturned()
    {
        // Create product, soft delete
        // Query
        // Assert deleted not in results
    }

    [Fact]
    public async Task Handle_TenantIsolation_OnlyCurrentTenant()
    {
        // Create products in multiple tenants
        // Query from tenant A
        // Assert only tenant A products
    }
}
```

**Total Query Tests:** ~40 tests

---

### Phase 4: Validator Tests

**Location:** `tests/KromicStore.Application.Tests/Features/Catalog/Validators/`

**Naming Convention:**
```
[CommandName]CommandValidatorTests.cs
```

**Test Pattern (FluentValidation):**
```csharp
public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        var repositoryMock = new Mock<IProductRepository>();
        _validator = new CreateProductCommandValidator(repositoryMock.Object);
    }

    [Fact]
    public void Validate_NameEmpty_HasError()
    {
        var command = new CreateProductCommand(
            categoryId: Guid.NewGuid(),
            name: "",  // Invalid
            sku: "TEST"
        );
        
        var result = _validator.Validate(command);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NameTooLong_HasError()
    {
        var longName = new string('A', 201);
        var command = new CreateProductCommand(
            categoryId: Guid.NewGuid(),
            name: longName,
            sku: "TEST"
        );
        
        var result = _validator.Validate(command);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var command = new CreateProductCommand(
            categoryId: Guid.NewGuid(),
            name: "Valid Name",
            sku: "VALID-001"
        );
        
        var result = _validator.Validate(command);
        
        result.IsValid.Should().BeTrue();
    }
}
```

**Total Validator Tests:** ~100 tests

---

### Phase 5: Authorization Tests

**Location:** `tests/KromicStore.Application.Tests/Features/Catalog/Authorization/`

**Test Pattern:**
```csharp
[Authorize(Roles = "TenantAdmin,StoreManager")]
public async Task AuthorizationTests()
{
    [Fact]
    public async Task CreateProduct_TenantAdmin_Allowed()
    {
        // Mock with TenantAdmin role
        // Execute command
        // Assert success
    }

    [Fact]
    public async Task CreateProduct_Customer_Denied()
    {
        // Mock with Customer role
        // Execute command
        // Assert access denied (401/403)
    }

    [Fact]
    public async Task RestoreCategory_TenantAdminOnly_StoreManagerDenied()
    {
        // Mock with StoreManager role
        // Execute RestoreCategory
        // Assert access denied
    }
}
```

**Total Authorization Tests:** ~20 tests

---

### Phase 6: Integration Tests

**Location:** `tests/KromicStore.Application.Tests/Features/Catalog/Integration/`

**Test Pattern:**
```csharp
public class CatalogIntegrationTests : IAsyncLifetime
{
    private readonly TestDatabaseFixture _fixture;

    public CatalogIntegrationTests()
    {
        _fixture = new TestDatabaseFixture();
    }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task CompleteProductLifecycle()
    {
        // Create product
        // Update product
        // Publish product
        // Archive product
        // Delete product
        // Restore product
        // Verify each step
    }

    [Fact]
    public async Task VariantManagementIntegration()
    {
        // Create product
        // Add variants
        // Update variant
        // Delete variant
        // Verify in database
    }

    [Fact]
    public async Task SoftDeleteAndRestore()
    {
        // Create product
        // Delete (soft delete)
        // Verify soft delete flags
        // Query (should not return)
        // Restore
        // Query (should return)
    }

    [Fact]
    public async Task MultiTenantIsolation()
    {
        // Create product in tenant A
        // Query from tenant B
        // Assert not visible
        // Create same SKU in tenant B
        // Assert both exist independently
    }
}
```

**Total Integration Tests:** ~20 tests

---

### Phase 7: Tenant Isolation Tests

**Location:** `tests/KromicStore.Application.Tests/Features/Catalog/TenantIsolation/`

**Test Pattern:**
```csharp
public class TenantIsolationTests
{
    [Fact]
    public async Task CreateProduct_DifferentTenants_IndependentData()
    {
        // Tenant A creates product SKU-001
        // Tenant B creates product SKU-001
        // Assert both exist
        // Query from A sees only A's
        // Query from B sees only B's
    }

    [Fact]
    public async Task UpdateProduct_CrossTenantDenied()
    {
        // Tenant A creates product
        // Tenant B attempts update
        // Assert update denied or product not found
    }
}
```

**Total Tenant Isolation Tests:** ~20 tests

---

## Test Execution Strategy

### Running Tests Locally

```bash
# All Phase 4 tests
dotnet test tests/KromicStore.Application.Tests/Features/Catalog/

# Domain tests only
dotnet test tests/KromicStore.Domain.Tests/Catalog/

# Specific command tests
dotnet test tests/KromicStore.Application.Tests/Features/Catalog/Commands/CreateProduct/

# With coverage reporting
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Test Framework Setup

**XUnit Configuration:**
```xml
<!-- xunit.runner.json -->
{
  "diagnosticMessages": false,
  "methodDisplay": "method",
  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

### Mocking Strategy

- **Repositories:** Mock with controlled returns
- **DbContext:** Mock for unit tests, in-memory for integration tests
- **Tenant Context:** Always mock to test isolation
- **Current User:** Mock to test audit fields and authorization
- **Logger:** Mock (can ignore in tests)

---

## Checklist for Implementation

### Before Writing First Test

- [ ] Create test project structure
- [ ] Setup XUnit + Fluent Assertions + Moq
- [ ] Create test fixtures and builders
- [ ] Setup in-memory database for integration tests
- [ ] Setup test base classes with common setup

### During Test Implementation

- [ ] Write test following matrix exactly
- [ ] One assertion per test (or closely related)
- [ ] Use meaningful test names
- [ ] Comment complex test logic
- [ ] Ensure deterministic results
- [ ] Use test builders for complex objects
- [ ] Clean up test data after each test

### After Test Implementation

- [ ] All tests passing (0 failures)
- [ ] Run full test suite
- [ ] Verify code coverage (target: >80% of critical paths)
- [ ] Check test execution time (< 30 sec total for unit tests)
- [ ] Review test code for duplication
- [ ] Get code review approval

---

## Success Criteria

**Phase 4 Testing Complete When:**

- [ ] All domain entity tests passing
- [ ] All command handler tests passing
- [ ] All validator tests passing
- [ ] All query handler tests passing
- [ ] All authorization tests passing
- [ ] All integration tests passing
- [ ] All tenant isolation tests passing
- [ ] All edge case tests passing
- [ ] Zero test failures
- [ ] Code review approved
- [ ] Test coverage >80% of critical paths

---

**Strategy Prepared:** July 30, 2026  
**Ready to Implement:** YES ✅  
**Estimated Timeline:** 3-4 weeks to implement all tests  
**Estimated Test Count:** 350+ tests

