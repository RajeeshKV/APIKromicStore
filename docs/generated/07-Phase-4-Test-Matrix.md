# Phase 4 Catalog - Comprehensive Test Matrix

**Date:** July 30, 2026  
**Purpose:** Requirement-driven test planning (before implementation)  
**Principle:** Every test validates a documented requirement  
**Status:** PLANNING PHASE

---

## Test Matrix Philosophy

- ✅ Test business behavior, not implementation details
- ✅ One requirement per test scenario
- ✅ Complete verification of success AND failure paths
- ✅ No duplicate tests
- ✅ Tests are living documentation of requirements
- ✅ Stop when all requirements are covered, not at a number target

---

## Part 1: Domain Entity Test Matrix

### 1.1 Product Domain Entity

**Requirement:** Product aggregate manages product lifecycle, variants, images, attributes, tags, and inventory.

#### Creation Tests (Product.Create)

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Create product with required fields | Name, SKU, CategoryId required | Product created with Draft status |
| Create product with all fields | Optional fields supported | All fields stored correctly |
| Create product initializes inventory | TrackInventory=true | ProductInventory created |
| Create with TrackInventory=false | Inventory tracking optional | ProductInventory not created |
| SKU format validation | SKU must be uppercase/numbers/hyphens | Invalid SKU rejected |
| Name length validation | Max 200 characters | Names exceeding limit rejected |
| Slug auto-generation | Generate from name if not provided | Slug auto-generated |
| Custom slug provided | Custom slug accepted | Custom slug used instead |
| Price validation | Price ≥ 0 | Negative price rejected |
| CompareAtPrice validation | Compare ≥ Price | Invalid compare price rejected |
| Dimensions validation | Must be > 0 if provided | Invalid dimensions rejected |
| Domain event raised | ProductCreatedEvent | Event captured in aggregate |
| Audit fields set | CreatedAtUtc, CreatedBy | Audit fields populated |
| Tenant context applied | TenantId set | Product isolated to tenant |

#### Status Transition Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Draft → Active (Publish) | Publish() transitions Draft→Active | Status changes to Active |
| Active → Archived (Archive) | Archive() sets Archived status | Status changes to Archived |
| Publish non-draft rejected | Cannot publish already active | Publish on Active is idempotent or rejected |
| Archive from any state | Can archive active or draft | Status becomes Archived |

#### Soft Delete Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| SoftDelete sets flags | IsDeleted=true, DeletedOnUtc, DeletedBy | All flags populated |
| Restore clears flags | Restore() resets soft delete | IsDeleted=false, DeletedOnUtc=null |
| Deleted products hidden in queries | Search excludes deleted | Deleted not returned |
| Can restore deleted product | Restore after delete | Product becomes active again |

#### Variant Management Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Add variant | AddVariant() creates variant | Variant added to collection |
| Variant SKU generated | SKU suffix combined with parent | Correct SKU format |
| Remove variant | RemoveVariant() removes | Variant removed from collection |
| Variant not found | RemoveVariant with invalid ID | InvalidOperationException |
| Multiple variants supported | Can add many variants | All variants retained |

#### Image Management Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Add image | AddImage() creates ProductImage | Image added to collection |
| Primary image flag | IsPrimary sets as primary | Primary image marked |
| Prevent duplicate primary | Only one primary allowed | Exception if adding second primary |
| Remove image | RemoveImage() removes | Image removed |
| Cannot remove last primary | At least one image required | Exception thrown |
| Set primary image | SetPrimaryImage() changes primary | Old primary unmarked, new marked |
| Image order maintained | Images maintain order | DisplayOrder correct |

#### Attribute Management Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Add attribute | AddAttribute() adds | Attribute stored |
| Attribute required fields | Name and value required | Empty values rejected |
| Duplicate attributes allowed | Can have same name | Not prevented in domain |
| Attribute validation | Non-null, non-empty strings | Invalid values rejected |

#### Tag Management Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Add tag | AddTag() creates tag | Tag added |
| Duplicate tag prevented | Cannot add same tag twice | InvalidOperationException |
| Remove tag | RemoveTag() removes | Tag removed |
| Tag not found | RemoveTag invalid tag | InvalidOperationException |
| Tag format | Non-null, non-empty | Invalid tags rejected |

#### Inventory Management Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Inventory created with product | ProductInventory initialized | Inventory object exists |
| Available quantity tracked | AvailableQuantity property | Correct quantity stored |
| Reserved quantity tracked | ReservedQuantity property | Correct quantity stored |
| GetAvailableStock calculation | Returns AvailableQuantity - ReservedQuantity | Correct calculation |

#### SKU & Slug Uniqueness Tests (Domain Level)

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| SKU value object creation | Validates SKU format | Invalid SKU rejected by VO |
| Slug value object creation | Validates slug format | Invalid slug rejected by VO |
| Slug auto-generation | Generate from name | Correct slug format |

---

### 1.2 Category Domain Entity

**Requirement:** Category aggregate manages product categories with hierarchical support.

#### Creation Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Create category | Create() factory method | Category created |
| Required fields | Name required | Empty name rejected |
| Optional fields | Description, Slug, ImageUrl optional | Defaults applied |
| Name length | Max 100 characters | Exceeding limit rejected |
| DisplayOrder default | Default=0 | DisplayOrder set to 0 |
| IsVisible default | Default=true | IsVisible set to true |
| Slug auto-generation | Generate from name | Correct slug |
| Custom slug | Custom slug accepted | Custom slug used |
| Parent category | ParentCategoryId optional | Hierarchy supported |

#### Hierarchy Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Parent-child relationship | Parent set via ParentCategoryId | Hierarchy established |
| Circular reference prevention | Category cannot be its own parent | InvalidOperationException |
| Update parent | Update() can change parent | Parent changed |
| Multi-level hierarchy | Parent of parent supported | Grandparent relationships work |

#### Soft Delete Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Soft delete category | SoftDelete() sets flags | IsDeleted=true |
| Archive behavior | Archive() hides category | IsVisible=false, Status=Archived |
| Restore category | Restore() resets | IsDeleted=false, removed from deleted |

---

### 1.3 ProductVariant Domain Entity

**Requirement:** ProductVariant represents product variants with SKU, price adjustments, and attributes.

#### Creation Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Create variant | Create() factory | Variant created |
| SKU required | Sku field required | Empty SKU rejected |
| Name required | Name field required | Empty name rejected |
| Price adjustment optional | PriceAdjustment default=0 | Default applied |
| Attributes storage | Dictionary of attributes | JSON storage |
| StockQuantity | Default=0 | Initial quantity set |
| IsActive | Default=true | Variant active by default |

#### Update Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Update variant name | Update() method | Name changed |
| Update price adjustment | Update() method | Price adjustment changed |
| Update attributes | Dictionary updated | Attributes changed |
| Toggle active | IsActive toggled | Active state changes |

---

### 1.4 ProductImage Domain Entity

**Requirement:** ProductImage manages product images with primary image flag and ordering.

#### Creation Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Create image | Create() factory | Image created |
| Cloudinary URL | ImageUrl required | URL stored |
| Alt text optional | AltText optional | Alt text supported |
| Primary flag | IsPrimary boolean | Primary flag set |
| Display order | DisplayOrder tracked | Order maintained |

#### Primary Image Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Set primary image | SetPrimary(true) | IsPrimary=true |
| Unset primary | SetPrimary(false) | IsPrimary=false |

---

### 1.5 ProductInventory Value Object

**Requirement:** ProductInventory tracks available and reserved quantities.

#### Calculation Tests

| Test Scenario | Business Rule | Success Criteria |
|---|---|---|
| Available stock calculation | AvailableQuantity - ReservedQuantity | Correct calculation |
| Negative available | Reserved > Available | Edge case handled |
| Reorder level | ReorderLevel threshold | Level stored |

---

## Part 2: Command Handler Test Matrix

**Note:** For each command, create tests covering:
1. Happy path (success scenario)
2. Validation failures (each validation rule)
3. Authorization failures (each role)
4. Business rule violations
5. Persistence verification
6. Response mapping
7. Tenant isolation
8. Soft delete behavior (where applicable)

### Commands Requiring Tests (18 Total)

| # | Command | Handler Tests | Validator Tests | Total Tests |
|---|---------|---|---|---|
| 1 | CreateProduct | 8 | 12 | 20 |
| 2 | UpdateProduct | 8 | 10 | 18 |
| 3 | CreateProductImage | 4 | 5 | 9 |
| 4 | DeleteProductImage | 3 | 3 | 6 |
| 5 | CreateVariant | 5 | 6 | 11 |
| 6 | UpdateVariant | 4 | 5 | 9 |
| 7 | DeleteVariant | 3 | 3 | 6 |
| 8 | AdjustInventory | 5 | 5 | 10 |
| 9 | DuplicateProduct | 6 | 4 | 10 |
| 10 | CreateCategory | 6 | 8 | 14 |
| 11 | UpdateCategory | 5 | 7 | 12 |
| 12 | DeleteCategory | 3 | 3 | 6 |
| 13 | RestoreCategory | 3 | 3 | 6 |
| 14 | CreateCollection | 5 | 6 | 11 |
| 15 | UpdateCollection | 4 | 5 | 9 |
| 16 | DeleteCollection | 3 | 3 | 6 |
| 17 | DeleteProduct | 3 | 3 | 6 |
| 18 | RestoreProduct | 3 | 3 | 6 |
| **TOTAL** | **18 commands** | **~80** | **~100** | **~180** |

### CreateProduct Command Tests

**Command:** `CreateProductCommand(CategoryId, Name, Sku, CustomSlug?, ShortDescription?, Description?, ProductType?, Status?, Price=0, CompareAtPrice?, CostPrice?, Weight?, Length?, Width?, Height?, IsFeatured=false, TrackInventory=true, Taxable=true, Attributes?, Tags?)`

#### Handler Tests (8 tests)

| Test | Scenario | Expected Result |
|---|---|---|
| CreateProduct_Success | Valid command, all fields | Product created, ID returned |
| CreateProduct_WithMinimalFields | Only required fields | Product created with defaults |
| CreateProduct_WithAllOptionalFields | All optional fields provided | All fields stored |
| CreateProduct_CategoryNotFound | Invalid CategoryId | Exception raised |
| CreateProduct_DuplicateSKU | SKU already exists in tenant | Duplicate SKU error |
| CreateProduct_DuplicateSlug | Custom slug already exists | Duplicate slug error |
| CreateProduct_TenantIsolation | Same SKU in different tenant | Both allowed (scoped to tenant) |
| CreateProduct_InventoryInitialized | TrackInventory=true | ProductInventory created |

#### Validator Tests (12 tests)

| Test | Scenario | Expected Result |
|---|---|---|
| Validator_NameRequired | Name empty | Validation fails |
| Validator_NameMaxLength | Name > 200 chars | Validation fails |
| Validator_SkuRequired | SKU empty | Validation fails |
| Validator_SkuMaxLength | SKU > 50 chars | Validation fails |
| Validator_SkuFormat | Invalid SKU format | Validation fails |
| Validator_CategoryIdRequired | CategoryId empty guid | Validation fails |
| Validator_PriceNotNegative | Price < 0 | Validation fails |
| Validator_CompareAtPriceGreater | CompareAt < Price | Validation fails |
| Validator_WeightPositive | Weight ≤ 0 | Validation fails |
| Validator_DimensionsPositive | Any dimension ≤ 0 | Validation fails |
| Validator_AttributesMaxCount | > 50 attributes | Validation fails |
| Validator_TagsMaxCount | > 20 tags | Validation fails |

---

## Part 3: Query Handler Test Matrix

### GetProducts Query Tests (Pagination, Filtering, Sorting)

| Test | Scenario | Expected Result |
|---|---|---|
| GetProducts_AllProducts | No filters | All active products returned |
| GetProducts_Pagination | Skip=10, Take=20 | Correct page returned |
| GetProducts_FilterByCategory | CategoryId provided | Only category products |
| GetProducts_FilterByStatus | Status provided | Only matching status |
| GetProducts_ExcludesDeleted | Soft-deleted products | Not in results |
| GetProducts_TenantIsolation | Multiple tenants | Only current tenant |
| GetProducts_EmptyResult | No matching products | Empty collection |
| GetProducts_MaxTakeLimit | Take > 100 | Limited to 100 |

### GetProductById Query Tests

| Test | Scenario | Expected Result |
|---|---|---|
| GetProductById_Exists | Valid ID | Product details returned |
| GetProductById_NotFound | Invalid ID | Null or 404 |
| GetProductById_Deleted | Soft-deleted product | Not returned |
| GetProductById_TenantIsolation | Different tenant's ID | Not returned |
| GetProductById_DTOProjection | DTO mapping | All fields mapped correctly |

### GetCategories Query Tests

| Test | Scenario | Expected Result |
|---|---|---|
| GetCategories_AllCategories | No filters | All active categories |
| GetCategories_Hierarchical | Parent categories | Hierarchy maintained |
| GetCategories_ExcludesDeleted | Soft-deleted | Not included |
| GetCategories_TenantIsolation | Multiple tenants | Only current tenant |

### SearchProducts Query Tests

| Test | Scenario | Expected Result |
|---|---|---|
| SearchProducts_TextMatch | Search term matches | Matching products |
| SearchProducts_CaseInsensitive | Mixed case search | Results found |
| SearchProducts_ExcludesDeleted | Deleted products | Not in results |
| SearchProducts_EmptySearch | Empty search term | All products or empty |
| SearchProducts_NoMatches | No matching products | Empty result |

---

## Part 4: Validator Test Matrix

### CreateProductCommandValidator Tests (12+ tests per requirement)

Each validation rule gets at minimum:
- Valid input test
- Invalid input test
- Boundary test
- Edge case test

Example - SKU Validation:
```
✅ ValidSKU_UppercaseLettersNumbers_Accepted
✅ ValidSKU_WithHyphens_Accepted
✅ ValidSKU_WithPeriods_Accepted
❌ InvalidSKU_Lowercase_Rejected
❌ InvalidSKU_SpecialCharacters_Rejected
❌ InvalidSKU_Empty_Rejected
❌ InvalidSKU_TooLong_Rejected
✅ ValidSKU_MaxLength_Accepted
✅ ValidSKU_SingleCharacter_Accepted
```

### UpdateProductCommandValidator Tests

Similar coverage for all update scenarios with optional fields:
- All null (no changes)
- Mix of null and provided values
- Invalid values for each field
- Boundary conditions

---

## Part 5: Authorization Test Matrix

### Command Authorization Tests (Every Command)

| Test | Scenario | Expected Result |
|---|---|---|
| Authorization_TenantAdmin | TenantAdmin role | Command allowed |
| Authorization_StoreManager | StoreManager role | Command allowed |
| Authorization_Customer | Customer role | Access denied |
| Authorization_Anonymous | No authorization | Access denied |
| Authorization_RestoreCategoryTenantAdminOnly | RestoreCategory + TenantAdmin | Allowed |
| Authorization_RestoreCategoryStoreManagerDenied | RestoreCategory + StoreManager | Denied (admin-only) |

---

## Part 6: Tenant Isolation Test Matrix

### Cross-Tenant Scenarios

For every command and query involving data access:

| Test | Scenario | Expected Result |
|---|---|---|
| TenantIsolation_CreateProduct_Tenant1 | Create product in tenant 1 | Product scoped to tenant 1 |
| TenantIsolation_QueryProduct_Tenant2 | Query from tenant 2 | Product not visible |
| TenantIsolation_UpdateProduct_CrossTenant | Update tenant 1 product from tenant 2 | Forbidden or not found |
| TenantIsolation_DeleteProduct_CrossTenant | Delete tenant 1 product from tenant 2 | Forbidden or not found |
| TenantIsolation_SKUUniquenessPerTenant | Same SKU in different tenants | Both allowed |
| TenantIsolation_SlugUniquenessPerTenant | Same slug in different tenants | Both allowed |

---

## Part 7: Soft Delete Test Matrix

### Soft Delete Behavior (18+ tests)

For every entity supporting soft delete (Product, Category, Variant, Variant):

| Test | Scenario | Expected Result |
|---|---|---|
| SoftDelete_Product_SetFlags | Delete product | IsDeleted=true, timestamps set |
| SoftDelete_QueryExclusion | Query after delete | Product not returned |
| SoftDelete_Restore | Restore deleted | IsDeleted=false, can query again |
| SoftDelete_DeletedImage | Delete image | Not in collection |
| SoftDelete_DeletedVariant | Delete variant | Not in collection |
| SoftDelete_SearchExclusion | Search after delete | Deleted not in results |

---

## Part 8: Business Rule Test Matrix

### Product Lifecycle (10+ tests)

| Test | Scenario | Expected Result |
|---|---|---|
| Lifecycle_Draft_DefaultStatus | New product | Status=Draft |
| Lifecycle_Draft_ToActive_Publish | Publish draft | Status=Active |
| Lifecycle_Active_ToArchived_Archive | Archive active | Status=Archived |
| Lifecycle_Archive_IsVisibleFalse | Archive product | IsVisible=false |
| Lifecycle_RestoreProduct_Reverses | Restore from delete | IsDeleted=false |

### Inventory Adjustment (8+ tests)

| Test | Scenario | Expected Result |
|---|---|---|
| Inventory_IncreaseQuantity | Positive adjustment | Quantity increased |
| Inventory_DecreaseQuantity | Negative adjustment | Quantity decreased |
| Inventory_CannotGoBelowZero | Decrease below 0 | Error or boundary handled |
| Inventory_AuditReason | Reason field | Reason stored |
| Inventory_ReservedTracking | Reserved quantity | Correctly tracked |

### Variant Uniqueness (5+ tests)

| Test | Scenario | Expected Result |
|---|---|---|
| VariantSKU_Unique | Variant SKU unique | Variant created |
| VariantSKU_Format | Parent-SKU + suffix | Correct format |
| VariantAttributes_Stored | Attributes dict | Stored as JSON |
| VariantPriceAdjustment | Positive/negative | Adjustment applied |

### Duplicate Product (8+ tests)

| Test | Scenario | Expected Result |
|---|---|---|
| Duplicate_CopiesAllData | Duplicate product | All fields copied |
| Duplicate_NewSKU_Required | Must provide SKU | Exception if not |
| Duplicate_NewSKU_Unique | New SKU must be unique | Error if duplicate |
| Duplicate_CopiesImages | Images copied | All images in new product |
| Duplicate_CopiesAttributes | Attributes copied | All attributes copied |
| Duplicate_CopiesTags | Tags copied | All tags copied |
| Duplicate_NewInventory | New inventory created | Independent inventory |
| Duplicate_DomainEvent | Event raised | ProductDuplicatedEvent captured |

---

## Part 9: Edge Cases & Boundary Tests

| Category | Edge Case | Test Name |
|---|---|---|
| **String Length** | Max length strings | ValidMaxLength_* tests |
| **Numeric Boundaries** | Zero, negative, max int | NumericBoundary_* tests |
| **Null/Empty** | All null fields, empty strings | EmptyAndNull_* tests |
| **Collections** | Empty lists, max size lists | CollectionBoundary_* tests |
| **Dates** | Very old/future dates | DateBoundary_* tests |
| **Concurrency** | Simultaneous operations | Concurrency_* tests |
| **Unicode** | Special characters | Unicode_* tests |

---

## Part 10: Integration Test Matrix

### End-to-End Scenarios

| Scenario | Steps | Verification |
|---|---|---|
| ProductLifecycle_Complete | Create → Update → Publish → Archive → Delete → Restore | All transitions work |
| VariantManagement_Complete | Create product → Add variants → Update variant → Delete variant | Variant ops work |
| InventoryTracking_Complete | Create → Adjust up → Adjust down → Query | Inventory correct |
| CategoryHierarchy_Complete | Create root → Create child → Update hierarchy | Hierarchy works |
| MultiTenantIsolation_Complete | Create data in T1 → Query from T2 → Verify isolation | Isolation enforced |
| SoftDeleteWorkflow_Complete | Create → Delete → Search (exclude) → Restore → Query | Soft delete works |

---

## Test Count Summary

| Phase | Component | Est. Tests |
|---|---|---|
| Phase 2 | Domain entities | 60 |
| Phase 3 | Command handlers | 80 |
| Phase 4 | Validators | 100 |
| Phase 5 | Query handlers | 30 |
| Phase 6 | Integration tests | 20 |
| Phase 7 | Multi-tenant tests | 20 |
| Phase 8 | Security/edge cases | 25 |
| Phase 9 | Regression suite | 15 |
| **TOTAL** | **All phases** | **~350** |

**NOTE:** Count driven by requirements, not arbitrary targets. Stop when requirements are covered.

---

**Matrix Completed:** July 30, 2026  
**Next Step:** Implement tests following this matrix  
**Principle:** Every test validates a documented requirement

