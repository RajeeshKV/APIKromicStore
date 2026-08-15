# CRITICAL FIX SUMMARY: User Segregation & Role Seeding

**Date:** 2026-08-15  
**Severity:** CRITICAL (Blocking All Admin Access + Security Risk)  
**Status:** Migration Created ✅ | Controllers TODO | Frontend TODO

---

## What Was Wrong

### Issue #1: Roles Never Seeded (Blocking All Access)

**Your JWT has NO role claim:**
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  // ❌ MISSING: "role": ["TenantAdmin"]
}
```

**Result:** ALL endpoints return 403 Forbidden

**Root Cause:**
1. Roles table empty (never seeded)
2. LoginCommandHandler queries for roles → returns empty list
3. GenerateAccessToken gets empty role list
4. JWT generated WITHOUT role claims
5. Every `[Authorize(Roles = "TenantAdmin")]` → 403

---

### Issue #2: No User Type Segregation

**Current problem:**
- SuperAdmin can call TenantAdmin endpoints (if auth passed)
- No separation between /api/v1/super/* and /api/v1/tenant/*
- Authorization only checks "do you have THIS role?" not "which role do you HAVE?"

**Security Risk:** Privilege escalation potential

---

## Solution Implemented

### Step 1: Seed Roles to Database ✅

**Status:** DONE

**File Created:** `src/KromicStore.Infrastructure/Persistence/Migrations/20260815_SeedRoles.cs`

**What it does:**
- Inserts 4 system roles into database
- SuperAdmin, TenantAdmin, StoreManager, Customer
- Runs automatically on next migration

**To apply:**
```bash
cd src/KromicStore.API
dotnet ef database update
```

---

### Step 2: Segregate API Routes (TODO)

**Status:** DOCUMENTATION READY, IMPLEMENTATION TODO

**What needs to change:**

#### Create Base Controller Classes

3 new files to create in `src/KromicStore.API/Controllers/BaseControllers/`:

**1. SuperAdminBaseController.cs**
```csharp
[ApiController]
[Route("api/v1/super")]
[Authorize(Roles = "SuperAdmin")]
public abstract class SuperAdminBaseController : ControllerBase { }
```

**2. TenantAdminBaseController.cs**
```csharp
[ApiController]
[Route("api/v1/tenant")]
[Authorize(Roles = "TenantAdmin")]
public abstract class TenantAdminBaseController : ControllerBase { }
```

**3. StoreManagerBaseController.cs**
```csharp
[ApiController]
[Route("api/v1/store-manager")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
public abstract class StoreManagerBaseController : ControllerBase { }
```

#### Update Existing Controllers

Change controllers to use appropriate base class:

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
}
// Routes become: /api/v1/tenant/categories
```

**Controllers to update:**
- CategoriesController → TenantAdminBaseController
- ProductsController → TenantAdminBaseController
- OrdersController → TenantAdminBaseController
- CustomersController → TenantAdminBaseController
- ThemeBuilderController → TenantAdminBaseController
- DashboardController → TenantAdminBaseController
- TenantsController (if exists) → SuperAdminBaseController
- SettingsController → TenantAdminBaseController

---

### Step 3: Update Frontend Routing (TODO)

**What needs to change:**

Frontend needs to detect user role and route accordingly:

```typescript
const roles = jwt_decode(token).role; // e.g., ["TenantAdmin"]

if (roles.includes("SuperAdmin")) {
  // Can only call /api/v1/super/* endpoints
  redirect to super admin panel
} else if (roles.includes("TenantAdmin")) {
  // Can only call /api/v1/tenant/* endpoints
  redirect to admin panel
} else if (roles.includes("Customer")) {
  // Can only call /api/v1/storefront/* endpoints
  redirect to storefront
}
```

---

## What Will Be Fixed

### Before (BROKEN)
```
❌ TenantAdmin logs in
❌ JWT has NO role claim
❌ Calls POST /api/v1/categories
❌ 403 Forbidden (because JWT has no role)
❌ All endpoints fail
❌ SuperAdmin could access TenantAdmin endpoints (if auth worked)
```

### After (FIXED)
```
✅ TenantAdmin logs in
✅ JWT has role: ["TenantAdmin"]
✅ Calls POST /api/v1/tenant/categories
✅ 200 OK (role matches)
✅ All endpoints work
✅ SuperAdmin cannot access TenantAdmin endpoints
   (tries to access /api/v1/tenant/categories)
   (JWT has role: ["SuperAdmin"])
   (Authorization fails: 403 Forbidden)
✅ Complete segregation enforced
```

---

## JWT After Fix

```json
// TenantAdmin Login
{
  "sub": "user-id",
  "email": "admin@store.com",
  "tenantId": "tenant-123",
  "isEmailVerified": true,
  "role": ["TenantAdmin"],      // ← NOW PRESENT (from migrated roles)
  "allowTenantIdBypass": true,
  "exp": 1786775079
}

// SuperAdmin Login
{
  "sub": "super-user-id",
  "email": "admin@kromic.in",
  "tenantId": null,
  "isEmailVerified": true,
  "role": ["SuperAdmin"],        // ← NOW PRESENT (from migrated roles)
  "allowTenantIdBypass": true,
  "exp": 1786775079
}
```

---

## Build Status

✅ **Build:** Succeeds (0 errors)  
✅ **Migration:** Created and compiles  
⏳ **Tests:** Need to run after controllers updated  
⏳ **Deployment:** Migration runs automatically on startup

---

## Implementation Order

1. **Apply migration** (automatic on startup)
   - Verify roles in database
   - Run: `dotnet ef database update`

2. **Create base controller classes** (3 new files)
   - SuperAdminBaseController
   - TenantAdminBaseController
   - StoreManagerBaseController

3. **Update existing controllers** (7-10 files)
   - Change inheritance
   - Remove route attributes (inherited from base)
   - Update authorization attributes (inherited from base)

4. **Update frontend** (routing logic)
   - Detect role in JWT
   - Route user to correct panel

5. **Test** (comprehensive)
   - TenantAdmin login → can access /api/v1/tenant/*
   - SuperAdmin login → can access /api/v1/super/*
   - TenantAdmin → 403 on /api/v1/super/* routes
   - SuperAdmin → 403 on /api/v1/tenant/* routes
   - Customer → 403 on admin routes

---

## Timeline

- **Migration:** ✅ 30 minutes (DONE)
- **Base Controllers:** 1 hour (TODO)
- **Controller Updates:** 2-3 hours (TODO)
- **Frontend Updates:** 1-2 hours (TODO)
- **Testing:** 1-2 hours (TODO)

**Total:** ~6-8 hours

---

## Next Steps

1. Verify build succeeds ✅ (Done)
2. Run migration to seed roles  
3. Create base controller classes
4. Update controllers to use base classes
5. Update frontend routing logic
6. Test end-to-end
7. Deploy

---

**Important:** After migration runs, your JWT will finally have role claims, and endpoints will work! 🎯

See `USER-SEGREGATION-AND-ROLE-FIX.md` for detailed implementation guide.
