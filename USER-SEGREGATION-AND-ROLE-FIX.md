# User Segregation & Role Seeding - Critical Security Fix

**Status:** IN PROGRESS  
**Date:** 2026-08-15  
**Severity:** CRITICAL (Security & Functionality)

---

## Problems Identified

### Problem 1: Roles Not Seeded to Database
**Impact:** JWT has NO role claims → ALL endpoints return 403 Forbidden

**Root Cause:**
- Roles (SuperAdmin, TenantAdmin, StoreManager, Customer) are defined in code
- But NEVER seeded into the database
- When LoginCommandHandler queries for roles, it finds nothing
- Empty roles list passed to GenerateAccessToken
- JWT generated with NO role claims
- All `[Authorize(Roles = "...")]` checks fail → 403

**Evidence:**
```json
// Your JWT payload:
{
  "sub": "user-id",
  "emailaddress": "user@example.com",
  "isEmailVerified": true,
  // ❌ MISSING: "role": ["TenantAdmin"] claim
}
```

### Problem 2: No User Type Segregation
**Impact:** SuperAdmin can login to TenantAdmin endpoints (security risk)

**Root Cause:**
- No separation between SuperAdmin and TenantAdmin routes/endpoints
- Frontend can send all users to same login endpoint
- Backend has no segregation between `/api/v1/super/*` and `/api/v1/tenant/*`
- Authorization only checks if user HAS role, not which role they SHOULD have

---

## Solution Architecture

### Three-Tier User Segregation

```
┌─────────────────────────────────────────────────────────┐
│  Admin Panel (Tenant Admin Only)                        │
│  - GET https://admin.kromic.in                          │
│  - Calls only /api/v1/tenant/* endpoints               │
│  - Auth: TenantAdmin role required                      │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  Super Admin Panel (SuperAdmin Only)                    │
│  - GET https://super.kromic.in                         │
│  - Calls only /api/v1/super/* endpoints                │
│  - Auth: SuperAdmin role required                       │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  Storefront (Customer Only)                             │
│  - GET https://www.kromic.in                           │
│  - Calls only /api/v1/storefront/* + public endpoints │
│  - Auth: Customer role (optional, for wishlist/cart)  │
└─────────────────────────────────────────────────────────┘
```

---

## Implementation Steps

### Step 1: Seed Roles (Database)

**Status:** ✅ DONE - Migration created

**File:** `src/KromicStore.Infrastructure/Persistence/Migrations/20260815_SeedRoles.cs`

```csharp
public partial class SeedRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Inserts 4 system roles:
        // - SuperAdmin
        // - TenantAdmin  
        // - StoreManager
        // - Customer
    }
}
```

**Run migration:**
```bash
dotnet ef database update
```

### Step 2: Segregate API Routes

**Current Problem:** All users can potentially access all endpoints

**Fix:** Create separate controller base classes with route prefixes

**Implementation:**

#### 2a. Create Base Controller Classes

**Location:** `src/KromicStore.API/Controllers/BaseControllers/` (NEW FOLDER)

**File 1:** `SuperAdminBaseController.cs`
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers.BaseControllers;

/// <summary>
/// Base class for Super Admin endpoints.
/// Only SuperAdmin role can access these endpoints.
/// Routes: /api/v1/super/*
/// </summary>
[ApiController]
[Route("api/v1/super")]
[Authorize(Roles = "SuperAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(void))]
public abstract class SuperAdminBaseController : ControllerBase
{
    protected readonly ILogger<SuperAdminBaseController> _logger;

    protected SuperAdminBaseController(ILogger<SuperAdminBaseController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

**File 2:** `TenantAdminBaseController.cs`
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers.BaseControllers;

/// <summary>
/// Base class for Tenant Admin endpoints.
/// Only TenantAdmin role can access these endpoints.
/// Routes: /api/v1/tenant/*
/// </summary>
[ApiController]
[Route("api/v1/tenant")]
[Authorize(Roles = "TenantAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(void))]
public abstract class TenantAdminBaseController : ControllerBase
{
    protected readonly ILogger<TenantAdminBaseController> _logger;

    protected TenantAdminBaseController(ILogger<TenantAdminBaseController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

**File 3:** `StoreManagerBaseController.cs`
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers.BaseControllers;

/// <summary>
/// Base class for Store Manager endpoints.
/// StoreManager and TenantAdmin can access these endpoints.
/// Routes: /api/v1/store-manager/*
/// </summary>
[ApiController]
[Route("api/v1/store-manager")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(void))]
public abstract class StoreManagerBaseController : ControllerBase
{
    protected readonly ILogger<StoreManagerBaseController> _logger;

    protected StoreManagerBaseController(ILogger<StoreManagerBaseController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

#### 2b. Update Existing Controllers

**Migration Plan:**

| Old Controller | New Base | New Route | New Role Check |
|---|---|---|---|
| `TenantsController` | `SuperAdminBaseController` | `/api/v1/super/tenants` | SuperAdmin only |
| `DashboardController` | `TenantAdminBaseController` | `/api/v1/tenant/dashboard` | TenantAdmin only |
| `CategoriesController` | `TenantAdminBaseController` | `/api/v1/tenant/categories` | TenantAdmin only |
| `ProductsController` | `TenantAdminBaseController` | `/api/v1/tenant/products` | TenantAdmin only |
| `OrdersController` | `TenantAdminBaseController` | `/api/v1/tenant/orders` | TenantAdmin,StoreManager |
| `CustomersController` | `TenantAdminBaseController` | `/api/v1/tenant/customers` | TenantAdmin only |
| `ThemesController` | `TenantAdminBaseController` | `/api/v1/tenant/themes` | TenantAdmin only |

**Example Migration (CategoriesController):**

```csharp
// BEFORE:
[ApiController]
[Route("api/v1/categories")]
[Authorize(Roles = "TenantAdmin")]
public class CategoriesController : ControllerBase { }

// AFTER:
[Route("categories")]
public class CategoriesController : TenantAdminBaseController
{
    public CategoriesController(ILogger<CategoriesController> logger) : base(logger) { }
    // routes become: /api/v1/tenant/categories
    // role check is automatic from base class
}
```

### Step 3: Update Frontend Routing

**Location:** Frontend apps need to detect role and route accordingly

**For Admin Panel:** (`web/web-admin`)
```typescript
// In App.tsx or Router component
const user = useCurrentUser();
const roles = jwt_decode(token).role; // e.g., ["TenantAdmin"]

if (roles?.includes("SuperAdmin")) {
  // Redirect to /api/v1/super/* endpoints
  navigate("/super-admin-panel");
} else if (roles?.includes("TenantAdmin")) {
  // Redirect to /api/v1/tenant/* endpoints
  navigate("/tenant-admin-panel");
} else {
  // Redirect to storefront
  navigate("/");
}
```

---

## Security Benefits

### Before
```
❌ SuperAdmin logs in
❌ Can call POST /api/v1/categories (TenantAdmin endpoint)
❌ Can call GET /api/v1/tenant/dashboard
❌ No segregation = security risk
```

### After
```
✅ SuperAdmin logs in
✅ JWT has role: ["SuperAdmin"]
✅ Tries to call POST /api/v1/categories
✅ Controller checks [Authorize(Roles = "TenantAdmin")]
✅ Authorization fails: 403 Forbidden
✅ SuperAdmin can ONLY access /api/v1/super/* endpoints
```

---

## JWT Role Claims After Fix

```json
// SuperAdmin Login
{
  "sub": "admin-user-id",
  "email": "admin@kromic.in",
  "tenantId": null,
  "isEmailVerified": true,
  "role": ["SuperAdmin"],  // ← NOW PRESENT
  "allowTenantIdBypass": true,
  "exp": 1786775079
}

// TenantAdmin Login
{
  "sub": "tenant-user-id",
  "email": "store@example.com",
  "tenantId": "tenant-id-123",
  "isEmailVerified": true,
  "role": ["TenantAdmin"],  // ← NOW PRESENT
  "allowTenantIdBypass": true,
  "exp": 1786775079
}

// Customer Login
{
  "sub": "customer-user-id",
  "email": "customer@example.com",
  "tenantId": "tenant-id-123",
  "isEmailVerified": true,
  "role": ["Customer"],  // ← NOW PRESENT
  "allowTenantIdBypass": true,
  "exp": 1786775079
}
```

---

## Deployment Checklist

### Database
- [ ] Run migration: `dotnet ef database update`
- [ ] Verify 4 roles created in Roles table

### Backend
- [ ] Create base controller classes
- [ ] Update all endpoint controllers to inherit from appropriate base class
- [ ] Verify routes changed (e.g., /api/v1/categories → /api/v1/tenant/categories)
- [ ] Rebuild and test: `dotnet build`
- [ ] Run tests: `dotnet test`

### Frontend
- [ ] Update API endpoints to use new route prefixes
- [ ] Update role-based routing logic
- [ ] Test TenantAdmin login → can only access tenant endpoints
- [ ] Test SuperAdmin login → can only access super endpoints
- [ ] Test Customer login → can only access storefront endpoints

### Testing
- [ ] SuperAdmin cannot login to Admin panel (wrong role)
- [ ] TenantAdmin cannot access /api/v1/super/* endpoints (403)
- [ ] TenantAdmin can access /api/v1/tenant/* endpoints (200)
- [ ] Customer cannot access /api/v1/tenant/* endpoints (403)
- [ ] JWT now contains correct role claim
- [ ] All auth checks work correctly

---

## Files to Create/Modify

### CREATE (New Files)
- ✅ `src/KromicStore.Infrastructure/Persistence/Migrations/20260815_SeedRoles.cs`
- ⏳ `src/KromicStore.API/Controllers/BaseControllers/SuperAdminBaseController.cs` (TODO)
- ⏳ `src/KromicStore.API/Controllers/BaseControllers/TenantAdminBaseController.cs` (TODO)
- ⏳ `src/KromicStore.API/Controllers/BaseControllers/StoreManagerBaseController.cs` (TODO)

### MODIFY (Existing Files)
- ⏳ All admin controllers to use base classes
- ⏳ Frontend routing logic
- ⏳ API endpoint configurations

---

## Why This Fixes the 403 Issue

**Current Flow (BROKEN):**
1. User logs in
2. LoginCommandHandler queries for roles → finds NONE (not seeded)
3. Empty roles list passed to GenerateAccessToken
4. JWT generated WITHOUT role claims
5. All endpoints with `[Authorize(Roles = "...")]` → 403 Forbidden

**After Fix:**
1. User logs in
2. LoginCommandHandler queries for roles → finds ["TenantAdmin"]
3. Role list passed to GenerateAccessToken
4. JWT generated WITH role claims: `"role": ["TenantAdmin"]`
5. Endpoints with `[Authorize(Roles = "TenantAdmin")]` → OK (200)
6. Endpoints with `[Authorize(Roles = "SuperAdmin")]` → 403 Forbidden (correct)

---

## Timeline

- **Migration Created:** ✅ Done
- **Base Controllers:** ⏳ Next (2-3 hours)
- **Controller Updates:** ⏳ Next (2-3 hours)
- **Testing:** ⏳ After (1-2 hours)
- **Frontend Updates:** ⏳ Parallel with backend

---

**Next Step:** Update existing controllers to use base classes and run migration.
