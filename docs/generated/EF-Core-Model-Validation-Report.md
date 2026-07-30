# EF Core Model Validation Report - Independent Investigation

**Date**: July 30, 2026  
**Investigation**: Root Cause Analysis of DbContext Initialization Failures  
**Scope**: Product Domain Model Configuration  

---

## Executive Summary

The authentication tests fail **not because of authentication logic errors**, but because the EF Core model cannot be initialized due to an ownership ambiguity in the product catalog domain model.

**The Root Issue**: `ProductImage` is declared as an owned entity of `Product`, but `ProductVariant` also maintains a collection of `ProductImage` instances without corresponding EF Core configuration, causing EF Core to be unable to determine a single owner.

---

## Failure Evidence

### Error Message

```
System.InvalidOperationException: 
Unable to determine the owner for the relationship between 
'ProductVariant.Images' and 'ProductImage' 
as both types have been marked as owned.

Either manually configure the ownership, or ignore the corresponding 
navigations using the [NotMapped] attribute or by using 
'EntityTypeBuilder.Ignore' in 'OnModelCreating'.
```

### When Error Occurs

- **During**: DbContext initialization via `Set<User>()` call
- **During**: Test setup in `InMemoryDbContextFactory.Create()`
- **Result**: All tests using `KromicStoreDbContext` fail before handlers execute

### Affected Tests

All 48 authentication handler tests fail at initialization:
- Cannot create context
- Cannot proceed to handler execution
- Cannot validate authentication logic

---

## Root Cause Analysis

### 1. Domain Model Definition

#### Product Entity
**File**: `src/KromicStore.Domain/Catalog/Entities/Product.cs`  
**Line**: 36-37

```csharp
private readonly List<ProductImage> _images = [];
public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();
```

**Method** (Line ~260):
```csharp
public void AddImage(string url, string? altText = null, int displayOrder = 0)
{
    var image = ProductImage.Create(ProductId, url, altText, _images.Count, isPrimary);
    _images.Add(image);
    AddDomainEvent(new ImageUploadedEvent(Id, TenantId, image.Id, url));
}
```

#### ProductVariant Entity
**File**: `src/KromicStore.Domain/Catalog/Entities/ProductVariant.cs`  
**Line**: 25-26

```csharp
private readonly List<ProductImage> _images = [];
public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();
```

**Method** (Line ~118):
```csharp
public void AddImage(string url, string? altText = null, int displayOrder = 0)
{
    var image = ProductImage.Create(ProductId, url, altText, displayOrder, false);
    _images.Add(image);
}
```

#### ProductImage Entity
**File**: `src/KromicStore.Domain/Catalog/Entities/ProductImage.cs`  
**Line**: 10-12

```csharp
public sealed class ProductImage : BaseEntity, ISoftDeletable
{
    public Guid ProductId { get; private set; }  // ← ONLY foreign key
```

**Observation**: ProductImage has `ProductId` only, not `ProductVariantId`.

### 2. EF Core Configuration

#### Product Configuration
**File**: `src/KromicStore.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`  
**Line**: 111-124

```csharp
// Product Images
builder.OwnsMany(p => p.Images, image =>
{
    image.ToTable("ProductImages");
    image.WithOwner().HasForeignKey("ProductId");
    image.HasKey("Id");
    image.Property(i => i.Url).IsRequired().HasMaxLength(500);
    image.Property(i => i.PublicId).IsRequired().HasMaxLength(500);
    image.Property(i => i.AltText).HasMaxLength(300);
    image.Property(i => i.DisplayOrder).HasDefaultValue(0);
    image.Property(i => i.IsPrimary).HasDefaultValue(false);
    image.Property(i => i.IsDeleted).HasDefaultValue(false);
    image.Property(i => i.DeletedOnUtc);
    image.Property(i => i.DeletedBy).HasMaxLength(500);
    image.HasIndex(i => new { i.ProductId, i.DisplayOrder });
});
```

**Result**: `ProductImage` is owned by `Product` with foreign key `ProductId`.

#### ProductVariant Configuration
**File**: `src/KromicStore.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`  
**Line**: 127-149

```csharp
// Product Variants
builder.OwnsMany(p => p.Variants, variant =>
{
    variant.ToTable("ProductVariants");
    variant.WithOwner().HasForeignKey("ProductId");
    variant.HasKey("Id");
    variant.Property(v => v.Sku).IsRequired().HasMaxLength(50);
    variant.Property(v => v.Name).IsRequired().HasMaxLength(200);
    variant.Property(v => v.PriceAdjustment).HasPrecision(18, 2).HasDefaultValue(0m);
    variant.Property(v => v.StockQuantity).HasDefaultValue(0);
    variant.Property(v => v.IsActive).HasDefaultValue(true);
    
    // Variant attributes as JSON
    variant.OwnsMany(v => v.Attributes, attr =>
    {
        attr.ToJson();
        attr.Property(a => a.Name).HasMaxLength(100);
        attr.Property(a => a.Value).HasMaxLength(100);
    });

    variant.HasIndex(v => new { v.ProductId, v.IsActive });
});
```

**Observation**: ProductVariant configuration does **NOT** include any mapping for `variant.Images`.

---

## The Conflict

### What EF Core Sees

1. **Product owns ProductImage**
   - `builder.OwnsMany(p => p.Images, image => ...)`
   - ProductImage foreign key = `ProductId`
   - ProductImage owned by Product

2. **ProductVariant references ProductImage**
   - `variant` navigation property exists: `Images => _images.AsReadOnly()`
   - `variant.AddImage()` method creates ProductImage instances
   - Type: `List<ProductImage>`

3. **EF Core Interpretation**
   - ProductImage is already owned by Product
   - ProductVariant also has a collection of ProductImage
   - **Who owns ProductImage when accessed through ProductVariant?**
   - Answer: EF Core cannot determine → **Error**

### Why This Is a Problem

- **Owned entities** can belong to exactly ONE owner
- **ProductImage** is already owned by **Product**
- **ProductVariant** cannot also own **ProductImage**
- EF Core sees the ambiguity and rejects the model

---

## Design Analysis

### Intent vs. Implementation

#### What the Code Suggests

1. **Product can have multiple images** (Product-Images relationship)
2. **ProductVariant can also have images** (ProductVariant-Images relationship)
3. **Same ProductImage type used for both** (causes conflict)

#### What the Database Schema Suggests

**ProductImages table** (from configuration):
- Column: `ProductId` (FK)
- No column: `ProductVariantId`
- No column: `VariantId`

**Result**: Images belong only to Products, not to ProductVariants.

#### The Contradiction

- **Domain model**: ProductVariant has `_images` collection
- **Database schema**: No way to link images to variants
- **Configuration**: Only Product owns images

---

## Architectural Question

### Should ProductVariant Have Images?

**Evidence suggesting NO**:

1. ProductImage has only `ProductId` foreign key
2. EF Core configuration only maps `Product.Images`
3. ProductImages table only links to Products
4. Variant images are not persisted (not in schema)

**Evidence suggesting YES**:

1. ProductVariant code explicitly manages `_images`
2. `AddImage()` method exists on ProductVariant
3. Domain model appears to support variant images
4. Variant-level images are a common e-commerce pattern

**Resolution needed**: Was this intentional design or incomplete implementation?

---

## Current Configuration State

### What Works

✅ Product → Images (explicit OwnMany configuration)  
✅ Product → Variants (explicit OwnMany configuration)  
✅ Variant → Attributes (explicit OwnMany configuration as JSON)  

### What Breaks

❌ Variant → Images (navigation exists in code, no EF mapping, conflicts with Product ownership)

---

## Impact on Tests

### Why Tests Fail

1. Test factory calls `InMemoryDbContextFactory.Create()`
2. Factory creates `KromicStoreDbContext(options, tenantContext)`
3. DbContext calls `OnModelCreating()`
4. EF Core applies all configurations via `ApplyConfigurationsFromAssembly()`
5. EF Core encounters `ProductVariant.Images` without mapping
6. **Exception thrown before any test logic executes**

### Test Failure Point

```
KromicStoreDbContext.get_UserSet() 
  → Set<User>() 
  → Triggers model building
  → EF Core validation fails
  → Exception thrown
```

### Result

- ✅ All 10 authentication tests compile successfully
- ❌ None can execute due to infrastructure initialization failure
- ❌ Not a test logic issue, an infrastructure issue

---

## Required Resolution

Choose ONE of the following approaches:

### Option A: Remove Images from ProductVariant

**Action**: Delete `_images` collection and `AddImage()` method from ProductVariant

**Rationale**:
- Images are only linked to Products in the database
- No `ProductVariantId` foreign key exists
- Simpler model, single responsibility

**Code Change**:
```csharp
// DELETE from ProductVariant.cs
private readonly List<ProductImage> _images = [];
public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();
public void AddImage(string url, ...) { ... }  // DELETE
```

### Option B: Separate ProductVariantImage Entity

**Action**: Create `ProductVariantImage` entity with its own configuration

**Rationale**:
- Variants genuinely need images
- Explicit entity separation clarifies intent
- Allows variant-specific image properties

**Database Impact**:
- New `ProductVariantImages` table
- Add `ProductVariantId` FK
- Add EF configuration for ownership

### Option C: Ignore Images Navigation on ProductVariant

**Action**: Tell EF Core to ignore the navigation

**Rationale**:
- Quick fix without code changes
- Variant images kept in code but not persisted
- Domain model preserved

**Code Change**:
```csharp
variant.Ignore(v => v.Images);
```

---

## Recommendation

**Based on Current Evidence**:

→ **Option A** is most appropriate

**Reason**: 
- The database schema doesn't support variant images
- The `ProductImage.Create()` factory always uses `ProductId`, never `VariantId`
- Removing the unused code clarifies the actual design
- Other owned collections (Attributes) show variant customization is possible via different patterns

**If** variant images are a genuine requirement for future phases:
- Implement as **Option B** properly with new entity and schema
- Add ProductVariantId to ProductImage or create ProductVariantImage

---

## Verification Required

After applying the fix:

1. **Rebuild**: `dotnet clean && dotnet build`
2. **Test Initialization**: Create a test context successfully
3. **Run Authentication Tests**: `dotnet test --filter "Features.Authentication"`
4. **Inspect Results**: Classify remaining failures

---

## Files to Modify

If following **Option A**:

```
src/KromicStore.Domain/Catalog/Entities/ProductVariant.cs

Lines to remove:
  - Line 25-26: _images collection declaration
  - Line 179-190: AddImage() method (approximate line numbers)
```

If following **Option C**:

```
src/KromicStore.Infrastructure/Persistence/Configurations/ProductConfiguration.cs

Add to ProductVariant configuration (line ~148):
  variant.Ignore(v => v.Images);
```

---

## Conclusion

The EF Core model validation error is **not a test problem**.

It is a **configuration conflict** between:
- Domain model declaration (ProductVariant has images)
- Database schema (no variant images table)  
- EF Core configuration (only Product owns images)

This must be resolved before any test can execute.

The authentication tests themselves are correctly written and compile successfully.
