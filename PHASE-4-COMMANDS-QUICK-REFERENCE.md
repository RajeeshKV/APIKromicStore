# Phase 4 Catalog Commands - Quick Reference

**Date:** July 30, 2026  
**Total Commands:** 18  
**Implementation Status:** All commands fully implemented with handlers, validators, and API endpoints

---

## Command Summary Table

| # | Command | Endpoint | Method | Key Properties | Auth | Complexity |
|---|---------|----------|--------|---|---|---|
| 1 | AdjustInventory | /api/v1/inventory/adjust | POST | ProductId, QuantityAdjustment, Reason | TA/SM | Low |
| 2 | CreateCategory | /api/v1/categories | POST | Name, Slug?, ParentCategoryId?, IsVisible=true | TA/SM | Medium |
| 3 | CreateCollection | /api/v1/collections | POST | Name, Description?, DisplayOrder=0, Status?=Active | TA/SM | Low |
| 4 | CreateProduct | /api/v1/products | POST | CategoryId, Name, Sku, Price=0, Dimensions?, Attributes?, Tags? | TA/SM | **High** |
| 5 | CreateProductImage | /api/v1/products/{id}/images | POST | ProductId, ImageUrl, AltText?, IsPrimary=false | TA/SM | Low |
| 6 | CreateVariant | /api/v1/products/{id}/variants | POST | ProductId, SkuSuffix, Name, PriceAdjustment=0, Attributes? | TA/SM | Medium |
| 7 | DeleteCategory | /api/v1/categories/{id} | DELETE | CategoryId | TA/SM | Low |
| 8 | DeleteCollection | /api/v1/collections/{id} | DELETE | CollectionId | TA/SM | Low |
| 9 | DeleteProduct | /api/v1/products/{id} | DELETE | ProductId | TA/SM | Low |
| 10 | DeleteProductImage | /api/v1/products/{id}/images/{imageId} | DELETE | ProductId, ImageId | TA/SM | Low |
| 11 | DeleteVariant | /api/v1/products/{id}/variants/{variantId} | DELETE | ProductId, VariantId | TA/SM | Low |
| 12 | DuplicateProduct | /api/v1/products/{id}/duplicate | POST | ProductId, NewSku, NewName, NewSlug? | TA/SM | Medium |
| 13 | RestoreCategory | /api/v1/categories/{id}/restore | POST | CategoryId | TA | Low |
| 14 | RestoreProduct | /api/v1/products/{id}/restore | POST | ProductId | TA/SM | Low |
| 15 | UpdateCategory | /api/v1/categories/{id} | PUT | CategoryId, Name?, Slug?, IsVisible?, ImageUrl? | TA/SM | Medium |
| 16 | UpdateCollection | /api/v1/collections/{id} | PUT | CollectionId, Name?, Description?, DisplayOrder?, Status? | TA/SM | Low |
| 17 | UpdateProduct | /api/v1/products/{id} | PUT | ProductId, Name?, Sku?, Price?, Dimensions?, Status? | TA/SM | **High** |
| 18 | UpdateVariant | /api/v1/products/{id}/variants/{variantId} | PUT | ProductId, VariantId, Name?, PriceAdjustment?, Attributes?, IsActive? | TA/SM | Medium |

**Legend:**
- TA = TenantAdmin
- SM = StoreManager
- Complexity: Low (1-3 props) | Medium (4-9 props) | High (10+ props)

---

## Command Categories

### Category Management (4 commands)
```
CreateCategory    → POST /api/v1/categories
UpdateCategory    → PUT /api/v1/categories/{id}
DeleteCategory    → DELETE /api/v1/categories/{id}
RestoreCategory   → POST /api/v1/categories/{id}/restore
```

### Product Management (7 commands)
```
CreateProduct     → POST /api/v1/products
UpdateProduct     → PUT /api/v1/products/{id}
DeleteProduct     → DELETE /api/v1/products/{id}
RestoreProduct    → POST /api/v1/products/{id}/restore
DuplicateProduct  → POST /api/v1/products/{id}/duplicate
CreateProductImage → POST /api/v1/products/{id}/images
DeleteProductImage → DELETE /api/v1/products/{id}/images/{imageId}
```

### Variant Management (3 commands)
```
CreateVariant     → POST /api/v1/products/{id}/variants
UpdateVariant     → PUT /api/v1/products/{id}/variants/{variantId}
DeleteVariant     → DELETE /api/v1/products/{id}/variants/{variantId}
```

### Collection Management (3 commands)
```
CreateCollection  → POST /api/v1/collections
UpdateCollection  → PUT /api/v1/collections/{id}
DeleteCollection  → DELETE /api/v1/collections/{id}
```

### Inventory Management (1 command)
```
AdjustInventory   → POST /api/v1/inventory/adjust
```

---

## Commands by Authorization Level

### TenantAdmin + StoreManager (17 commands)
All commands except RestoreCategory

### TenantAdmin Only (1 command)
- RestoreCategory (admin-only restore operation)

---

## Property Field Count by Command

| Count | Commands | Examples |
|-------|----------|----------|
| **1** | 4 | DeleteCategory, DeleteCollection, DeleteProduct, RestoreCategory, RestoreProduct |
| **2** | 2 | DeleteProductImage, DeleteVariant |
| **3** | 1 | AdjustInventory |
| **4** | 3 | CreateProductImage, DuplicateProduct, UpdateCollection |
| **6** | 2 | CreateVariant, UpdateVariant |
| **7** | 1 | CreateCollection |
| **8** | 2 | CreateCategory, UpdateCategory |
| **16** | 1 | UpdateProduct |
| **20** | 1 | CreateProduct |

---

## Soft Delete vs Hard Delete

### Soft Deletes (6 commands)
- DeleteCategory - marked as deleted, RestoreCategory available
- DeleteCollection - marked as deleted, no restore endpoint
- DeleteProduct - marked as deleted, RestoreProduct available
- DeleteProductImage - references deleted
- DeleteVariant - marked as deleted
- RestoreCategory / RestoreProduct - restore from soft delete

### Hard Deletes (1 potential)
- DeleteProductImage - may hard delete (image reference cleanup)

---

## Complex Command Properties

### CreateProduct (20 fields)
```
Required: CategoryId, Name, Sku
Default: ProductType=Physical, Status=Draft, Price=0, IsFeatured=false, 
         TrackInventory=true, Taxable=true
Optional: CustomSlug, ShortDescription, Description, CompareAtPrice, CostPrice,
         Weight, Length, Width, Height, Attributes, Tags
```

### UpdateProduct (16 fields - all optional)
```
Required: ProductId
Optional: CategoryId, Name, Sku, CustomSlug, ShortDescription, Description,
         Status, Price, CompareAtPrice, CostPrice, Weight, Length, Width,
         Height, IsFeatured, Taxable
```

### CreateCategory (8 fields)
```
Required: Name
Default: DisplayOrder=0, IsVisible=true
Optional: Description, Slug, ParentCategoryId, ImageUrl
```

### UpdateCategory (8 fields - all optional except CategoryId)
```
Required: CategoryId
Optional: Name, Description, Slug, ParentCategoryId, DisplayOrder, IsVisible, ImageUrl
```

---

## Validation Constraints by Command

### Unique Constraints
- **CreateProduct**: SKU must be unique within tenant
- **UpdateProduct**: SKU must be unique if changed within tenant
- **DuplicateProduct**: NewSku must be unique within tenant
- **CreateCategory**: Slug must be unique if provided within tenant
- **UpdateCategory**: Slug must be unique if changed within tenant

### Range Constraints
- **AdjustInventory**: QuantityAdjustment can be positive or negative
- **CreateVariant**: StockQuantity ≥ 0 (assumed)
- **CreateProduct**: Price ≥ 0 (assumed)

### Required References
- **CreateProduct**: CategoryId required (Category must exist)
- **CreateVariant**: ProductId required (Product must exist)
- **DuplicateProduct**: ProductId required (Product must exist)

### Enum Values
- **CreateProduct**: ProductType (default="Physical"), Status (default="Draft")
- **CreateCollection**: Status (default="Active")
- **UpdateProduct**: Status can change
- **UpdateCollection**: Status can change

---

## Handler/Validator Naming Convention

All follow standard pattern:
```
Command: CreateProductCommand
Handler: CreateProductCommandHandler
Validator: CreateProductCommandValidator
Response: CreateProductResponse
```

---

## API Response Objects

Each command returns a response DTO with subset of created/updated entity fields:

| Command | Response DTO | Fields |
|---------|---|---|
| AdjustInventory | AdjustInventoryResponse | ProductId, NewAvailableQuantity, ReservedQuantity, Message |
| CreateCategory | CreateCategoryResponse | CategoryId, Name, Slug |
| CreateCollection | CreateCollectionResponse | CollectionId, Name, Status |
| CreateProduct | CreateProductResponse | ProductId, Name, Sku, Slug |
| CreateProductImage | CreateProductImageResponse | ImageId, ProductId, ImageUrl, IsPrimary |
| CreateVariant | CreateVariantResponse | VariantId, ProductId, Name, Sku |
| DeleteCategory | DeleteCategoryResponse | CategoryId, Message |
| DeleteCollection | DeleteCollectionResponse | CollectionId, Message |
| DeleteProduct | DeleteProductResponse | ProductId, Message |
| DeleteProductImage | DeleteProductImageResponse | ImageId, Message |
| DeleteVariant | DeleteVariantResponse | VariantId, Message |
| DuplicateProduct | DuplicateProductResponse | DuplicatedProductId, NewSku, NewName, NewSlug |
| RestoreCategory | RestoreCategoryResponse | CategoryId, Message |
| RestoreProduct | RestoreProductResponse | ProductId, Message |
| UpdateCategory | UpdateCategoryResponse | CategoryId, Name, Slug |
| UpdateCollection | UpdateCollectionResponse | CollectionId, Name, Status |
| UpdateProduct | UpdateProductResponse | ProductId, Name, Sku, Slug |
| UpdateVariant | UpdateVariantResponse | VariantId, ProductId, Name, Sku |

---

## Implementation Status Verification

### ✅ Fully Implemented
- All 18 commands have record definitions
- All 18 have CommandHandler classes
- All 18 have CommandValidator classes
- All 18 have API endpoints in controllers
- All 18 have authorization attributes
- All 18 have response DTOs

### ⚠️ Missing Test Coverage
- **0 command handler tests**
- **0 validator tests**
- **0 integration tests**

### 📝 Critical Finding
Phase 4 implementation is structurally complete but functionally unverified. No tests have been written for any of the 18 catalog commands.

---

## Mapping to Doc 27 - Catalog APIs

All 18 commands map directly to endpoints defined in Doc 27:

✅ Categories: 4/4 command endpoints mapped  
✅ Products: 7/7 command endpoints mapped  
✅ Variants: 3/3 command endpoints mapped  
✅ Collections: 3/3 command endpoints mapped  
✅ Inventory: 1/1 command endpoints mapped  
✅ Images: 2/2 command endpoints mapped  

**Unmapped Doc 27 Endpoints:**
- GET /api/v1/categories (Query, not Command)
- GET /api/v1/categories/{id} (Query, not Command)
- GET /api/v1/products (Query, not Command)
- GET /api/v1/products/{id} (Query, not Command)
- GET /api/v1/products/{id}/variants (Query, not Command)
- GET /api/v1/collections (Query, not Command)
- GET /api/v1/inventory (Query, not Command)
- PUT /api/v1/products/{id}/images/order (Query-based image reordering)

---

## File Locations

### Command Files
- `src/KromicStore.Application/Features/Catalog/Commands/[CommandName]/[CommandName]Command.cs`

### Handler Files
- `src/KromicStore.Application/Features/Catalog/Commands/[CommandName]/[CommandName]CommandHandler.cs`

### Validator Files
- `src/KromicStore.Application/Features/Catalog/Commands/[CommandName]/[CommandName]CommandValidator.cs`

### Controller Files
- `src/KromicStore.API/Controllers/CategoriesController.cs`
- `src/KromicStore.API/Controllers/ProductsController.cs`
- `src/KromicStore.API/Controllers/VariantsController.cs`
- `src/KromicStore.API/Controllers/CollectionsController.cs`
- `src/KromicStore.API/Controllers/InventoryController.cs`

---

## Next Steps for Complete Implementation

1. **Add Comprehensive Test Suite** (CRITICAL)
   - ~80 command handler tests
   - ~90 validator tests
   - ~20 integration tests

2. **Verify Complex Business Rules**
   - Inventory adjustment auditing
   - Duplicate product media copy
   - Soft delete behavior
   - Restore authorization

3. **Performance Testing**
   - Bulk operations performance
   - Large product attribute handling
   - Image optimization

4. **Security Audit**
   - Authorization enforcement
   - Input validation completeness
   - SQL injection prevention
   - Tenant isolation verification

---

**Quick Reference Generated:** July 30, 2026  
**For Detailed Mapping:** See PHASE-4-COMMANDS-MAPPING.md  
**For CSV Format:** See PHASE-4-COMMANDS-TRACEABILITY-MATRIX.csv

