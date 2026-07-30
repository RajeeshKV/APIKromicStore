# Phase 2 Authentication Test Completion - Fix Strategy

**Status:** Task #1 Complete - Error Analysis & Pattern Mapping

---

## Error Classification Summary

**Total Errors Analyzed:** 290+  
**Pattern Categories Identified:** 9

| Error Pattern | Count | Impact | Fix Strategy |
|---|---|---|---|
| **CS0103:** InMemoryDbContextFactory not found | ~10 | Critical | Add using statement (already in factory) |
| **CS7036:** Missing constructor parameters | ~60 | Critical | Add ILogger, ICurrentUserService, ITokenService params |
| **CS1061:** Missing User methods | ~40 | High | Methods renamed (MarkEmailVerified, AddRefreshToken not exposed) |
| **CS1061:** Private TenantContext access | ~25 | High | Refactor tests to avoid private field access |
| **CS1061:** IQueryable.Add() not available | ~80 | High | Use DbSet instead of IQueryable in factory |
| **CS0815:** Void assignment to var | ~5 | Medium | Remove variable assignment or use statement |
| **CS1061:** Wrong DTO properties | ~10 | Medium | Use correct property names (User.Id not UserId) |
| **CS1729:** Constructor argument mismatch | ~10 | Medium | Fix query/command construction |
| **CS0246:** Missing using (ILogger<>) | ~1 | Low | Add using for Microsoft.Extensions.Logging |

---

## Root Causes

### Root Cause #1: InMemoryDbContextFactory Using Statement Missing

**Error:** CS0103: The name 'InMemoryDbContextFactory' does not exist  
**Location:** All test files in constructor  
**Why:** Factory exists in `Common/` subfolder but tests don't import it

**Solution:** Add using statement to each test file:
```csharp
using KromicStore.Application.Tests.Common;
```

**Impact:** Fix ~10 errors immediately

---

### Root Cause #2: Missing ILogger Parameter in Handlers

**Error:** CS7036: There is no argument given that corresponds to the required parameter 'logger'

**Handlers Affected:**
- LoginCommandHandler - needs `ILogger<LoginCommandHandler>`
- RegisterCommandHandler - needs `ILogger<RegisterCommandHandler>`
- RefreshTokenCommandHandler - needs `ILogger<RefreshTokenCommandHandler>`
- VerifyEmailCommandHandler - needs `ILogger<VerifyEmailCommandHandler>`
- ForgotPasswordCommandHandler - needs `ILogger<ForgotPasswordCommandHandler>`
- ResetPasswordCommandHandler - needs `ILogger<ResetPasswordCommandHandler>`
- ChangePasswordCommandHandler - needs `ILogger<ChangePasswordCommandHandler>`
- LogoutCommandHandler - needs `ILogger<LogoutCommandHandler>`
- GetCurrentUserQueryHandler - needs `ICurrentUserService`

**Solution:** Update test constructors to create mock ILogger:
```csharp
private readonly ILogger<LoginCommandHandler> _logger;

public LoginCommandHandlerTests()
{
    _logger = Substitute.For<ILogger<LoginCommandHandler>>();
    _sut = new LoginCommandHandler(_dbContext, _passwordHasher, _tokenService, _logger);
}
```

**Impact:** Fix ~60 errors

---

### Root Cause #3: Private TenantContext Access

**Error:** CS1061: 'KromicStoreDbContext' does not contain a definition for 'TenantContext'

**Location:** All tests that try to access `_dbContext.TenantContext`

**Current Code Pattern (WRONG):**
```csharp
var user = User.CreateTenantUser(
    tenantId: _dbContext.TenantContext.TenantId!.Value,  // ← WRONG: Private field
    ...
);
```

**Solution:** Use factory-provided TestTenantContext

**How Factory Works:**
```csharp
public static KromicStoreDbContext Create(Guid? tenantId = null)
{
    // ...
    return new KromicStoreDbContext(options, new TestTenantContext(tenantId));
}
```

**Fix Pattern:** Don't access TenantContext at all. Use factory correctly:
```csharp
var tenantId = Guid.NewGuid();
var _dbContext = InMemoryDbContextFactory.Create(tenantId);
// TenantContext is now set up internally via TestTenantContext

// Create user WITHOUT accessing _dbContext.TenantContext
var user = User.CreateTenantUser(
    tenantId: tenantId,  // Use the same tenantId passed to factory
    ...
);
```

**Impact:** Fix ~25 errors

---

### Root Cause #4: User Methods Don't Exist

**Error:** CS1061: 'User' does not contain a definition for 'AddRefreshToken'

**Missing Methods in Tests:**
- `User.AddRefreshToken()` - Not a public method
- `User.AddEmailVerificationToken()` - Not a public method
- `User.AddPasswordResetToken()` - Not a public method
- `User.VerifyEmail()` - Actually named `MarkEmailVerified()`

**Analysis:** These methods likely don't exist because tokens are managed separately (RefreshToken, EmailVerificationToken, PasswordResetToken entities).

**Solution:** Don't add tokens manually in test setup. Instead:

1. If tests need tokens, create entities directly:
```csharp
var refreshToken = RefreshToken.Create(
    userId: user.Id,
    tokenHash: "hash",
    expiresOnUtc: DateTime.UtcNow.AddDays(7));
_dbContext.RefreshTokens.Add(refreshToken);
```

2. If tests need email verified, use correct method:
```csharp
user.MarkEmailVerified();  // Not user.VerifyEmail()
```

**Impact:** Fix ~40 errors

---

### Root Cause #5: IQueryable.Add() Not Available

**Error:** CS1061: 'IQueryable<User>' does not contain a definition for 'Add'

**Location:** Tests calling `_dbContext.Users.Add(user)`

**Problem:** DbContext properties are currently defined as `IQueryable<T>`:
```csharp
public IQueryable<User> Users => UserSet;  // ← Returns IQueryable, not DbSet
```

**But IQueryable doesn't have Add() - only DbSet does.**

**Current Factory Implementation Issue:** Tests are using DbSet directly in inline code, which works, but factory design inconsistency.

**Solution:** Use DbSet explicitly OR change DbContext to expose DbSet:

**Option A (Recommended):** Refactor DbContext properties
```csharp
// CURRENT (wrong for Add)
public IQueryable<User> Users => UserSet;

// BETTER
public DbSet<User> Users => UserSet;
```

**Option B:** Use UserSet directly in tests
```csharp
_dbContext.UserSet.Add(user);  // UserSet is DbSet<User>
```

**Option C (Minimal Change):** Avoid manual add - let factory handle
```csharp
// Don't do this:
_dbContext.Users.Add(user);

// Do this instead:
_dbContext.UserSet.Add(user);
```

**Impact:** Fix ~80 errors

---

## Test Files & Fixes Needed

### LoginCommandHandlerTests.cs

**Errors:**
- Missing ILogger parameter
- Missing using statement  
- Private TenantContext access
- IQueryable.Add() error
- Wrong property access (UserId)

**Fixes:**
1. Add `using KromicStore.Application.Tests.Common;`
2. Create mock `_logger`
3. Pass logger to constructor
4. Change `_dbContext.TenantContext.TenantId` to use passed tenantId
5. Use `_dbContext.UserSet.Add()` instead of `_dbContext.Users.Add()`
6. Change `result.UserId` to `result.User.Id`

---

### RegisterCommandHandlerTests.cs

**Same patterns as LoginCommandHandlerTests**

**Additional Issues:**
- Wrong DTO property access (UserId)

---

### RefreshTokenCommandHandlerTests.cs

**Errors:**
- Missing ILogger parameter
- Private TenantContext access
- User.VerifyEmail() wrong name
- User.AddRefreshToken() doesn't exist
- Missing 'roles' parameter in GenerateAccessToken

**Fixes:**
1. Add using statement
2. Create mock _logger
3. Replace private TenantContext access
4. Change `user.VerifyEmail()` to `user.MarkEmailVerified()`
5. Remove manual AddRefreshToken - create RefreshToken entity if needed
6. Add `roles` parameter to GenerateAccessToken call

---

### LogoutCommandHandlerTests.cs

**Errors:**
- Missing ITokenService parameter
- Private TenantContext access
- User.AddRefreshToken() doesn't exist
- RefreshToken.Revoke() signature mismatch

**Fixes:**
1. Add using statement
2. Create mock _tokenService
3. Use correct tenantId approach
4. Create RefreshToken entities manually if needed
5. Check RefreshToken.Revoke() signature for required parameter

---

### VerifyEmailCommandHandlerTests.cs

**Errors:**
- Missing ILogger parameter
- Private TenantContext access
- User.AddEmailVerificationToken() doesn't exist
- Void assignment to var

**Fixes:**
1. Add using statement
2. Create mock _logger
3. Use correct tenantId approach
4. Remove manual token setup - create EmailVerificationToken entity
5. Change `var result = user.MarkEmailVerified();` to statement form

---

### ForgotPasswordCommandHandlerTests.cs

**Errors:**
- Missing ILogger parameter
- User.AddPasswordResetToken() doesn't exist
- Private TenantContext access

**Fixes:**
1. Add using statement
2. Create mock _logger
3. Create PasswordResetToken entity manually if needed

---

### ChangePasswordCommandHandlerTests.cs

**Errors:**
- Missing ILogger and ICurrentUserService parameters
- User token methods don't exist
- Private TenantContext access

**Fixes:**
1. Add using statement
2. Create mocks for _logger and _currentUserService
3. Refactor token setup

---

### GetCurrentUserQueryHandlerTests.cs

**Errors:**
- Missing ICurrentUserService parameter
- GetCurrentUserQuery constructor issue
- UserProfileResponse property names wrong

**Fixes:**
1. Add using statement
2. Create mock _currentUserService
3. Check GetCurrentUserQuery constructor signature
4. Use correct UserProfileResponse properties

---

### ResendVerificationEmailCommandHandlerTests.cs

**Errors:**
- Missing ILogger parameter
- Private TenantContext access

**Fixes:**
1. Add using statement
2. Create mock _logger
3. Refactor tenantId access

---

## Implementation Priority

### Priority 1: Core Fixes (Applies to ALL files)
1. Add using statements
2. Add missing logger/service mocks
3. Update constructors

### Priority 2: Structural Fixes (Affects multiple files)
1. Fix DbContext property access (UserSet vs Users)
2. Fix TenantContext access pattern
3. Fix User method names and token handling

### Priority 3: Specific Fixes (Per-file)
1. DTO property access corrections
2. Query/Command constructor signatures
3. Method parameter matching

---

## Recommended Implementation Order

1. **Fix all using statements** - This fixes 10+ errors immediately
2. **Add all logger/service mocks** - This fixes 60+ errors
3. **Fix DbContext patterns** - This fixes 80+ errors
4. **Fix method names and property access** - This fixes remaining errors
5. **Test infrastructure refinement** - Ensure factory and helpers are complete

---

## Expected Result

**Before Fixes:** 290+ compilation errors  
**After Fixes:** 0 compilation errors, tests compile and run

**Next Step (Task #2):** Begin implementing fixes starting with LoginCommandHandlerTests

---

