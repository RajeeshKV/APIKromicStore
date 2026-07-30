# Phase 4 - Evidence-Based Investigation

**Date:** July 30, 2026  
**Status:** UNDER INVESTIGATION - Systematic Analysis

---

## Executive Summary

Previous reports assumed test errors belonged to Phase 5. This investigation systematically verifies every claim against documented evidence and source code.

**Key Finding:** Authentication handlers DO EXIST. They are Phase 2 code, already implemented. The failing tests are testing Phase 2 code, but contain errors that prevent compilation.

**Next Step:** Determine if Phase 4 broke Phase 2 tests, or if tests have pre-existing bugs.

---

## Section 1: Test Ownership - WHERE IS AUTHENTICATION DEFINED?

### Claim: "Authentication is Phase 5"

**Investigation:**

1. **Documentation Evidence:**

File: `IMPLEMENTATION-SUMMARY-PHASE-2-3.md`
```
Status:** ✅ **PRODUCTION-READY — APPROVED FOR PHASE 4**

## Session Summary

This session completed all remaining Phase 2 (Authentication) 
and Phase 3 (Tenant Management) work to production-ready status.
```

**Finding:** According to official documentation, **Authentication is Phase 2, NOT Phase 5**.

**Evidence:** The same document lists:
- Phase 2 Authentication handlers: 9 implemented
- Phase 3 Tenant Management: 9 implemented
- Phase 4: Product Catalog (current)

---

## Section 2: Build History - WERE TESTS COMPILING BEFORE PHASE 4?

### Question: Did Phase 2 tests compile successfully before Phase 4 began?

**Investigation:**

File: `PHASE-2-3-COMPLETION-ROADMAP.md` shows:

```
### ⏳ PENDING (Test Binding & Integration)

**Handler Test Fixes Needed**

Current Issue: Tests use concrete `KromicStoreDbContext` but 
handlers expect `IApplicationDbContext` interface.
```

**Finding:** Tests had known compilation issues BEFORE Phase 4.

**Evidence:** The Phase 2-3 roadmap explicitly states handler tests need mocking pattern fixes to compile.

**Conclusion:** These tests were NOT successfully compiling before Phase 4 started.

---

## Section 3: Authentication Handlers - DO THEY EXIST?

### Question: Are the authentication handlers already implemented?

**Investigation:**

Directory: `src/KromicStore.Application/Features/Authentication/`

**Handler Files Found:**
- ✅ `Commands/Login/LoginCommandHandler.cs` - EXISTS
- ✅ `Commands/Register/RegisterCommandHandler.cs` - EXISTS
- ✅ `Commands/RefreshToken/RefreshTokenCommandHandler.cs` - EXISTS
- ✅ `Commands/Logout/LogoutCommandHandler.cs` - EXISTS
- ✅ `Commands/VerifyEmail/VerifyEmailCommandHandler.cs` - EXISTS
- ✅ `Commands/ForgotPassword/ForgotPasswordCommandHandler.cs` - EXISTS
- ✅ `Commands/ResetPassword/ResetPasswordCommandHandler.cs` - EXISTS
- ✅ `Commands/ChangePassword/ChangePasswordCommandHandler.cs` - EXISTS
- ✅ `Queries/GetCurrentUser/GetCurrentUserQueryHandler.cs` - EXISTS

**Finding:** All 9 authentication command handlers already exist and are implemented.

**Status:** PHASE 2 CODE - ALREADY PRODUCED.

---

## Section 4: Regression Analysis - DID PHASE 4 BREAK PHASE 2?

### Question: Did Phase 4 introduce changes that broke Phase 2 APIs?

#### Test 1: LoginCommandHandler Constructor

**Current Handler Signature:**
```csharp
public LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher       passwordHasher,
    ITokenService         tokenService,
    ILogger<LoginCommandHandler> logger)
```

**Test Code Trying:**
```csharp
_sut = new LoginCommandHandler(_dbContext, _passwordHasher, _tokenService);
```

**Finding:** Test is missing `ILogger<LoginCommandHandler>` parameter.

**Classification:** Test is outdated OR intentionally incomplete.

**Phase 4 Involvement:** Phase 4 does NOT use ILogger. Phase 2 handlers require it. This is NOT a Phase 4 regression.

---

#### Test 2: User.VerifyEmail() Method

**Current Method in User.cs:**
```csharp
public void MarkEmailVerified() => IsEmailVerified = true;
```

**Test Code Calling:**
```csharp
user.VerifyEmail();
```

**Finding:** Method is `MarkEmailVerified()`, NOT `VerifyEmail()`.

**Classification:** Test has incorrect method name.

**Phase 4 Involvement:** This method existed since Phase 2. Phase 4 did not rename it. NOT a Phase 4 regression.

---

#### Test 3: AuthTokenResponse.UserId Property

**Current DTO:**
```csharp
public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int    ExpiresInSeconds,
    UserProfileResponse User
);
```

**Test Code Accessing:**
```csharp
result.UserId.Should().Be(user.Id);
```

**Finding:** AuthTokenResponse does NOT have `UserId` property. It has a `User` (UserProfileResponse) property instead.

**Classification:** Test has incorrect property name.

**Phase 4 Involvement:** AuthTokenResponse structure was defined in Phase 2. Phase 4 did not modify it. NOT a Phase 4 regression.

---

#### Test 4: DbContext.TenantContext Property

**Current DbContext Code:**
```csharp
private readonly ITenantContext _tenantContext;

// No public property named "TenantContext"
```

**Test Code Accessing:**
```csharp
var user = User.CreateTenantUser(
    tenantId: _dbContext.TenantContext.TenantId!.Value,
    ...
);
```

**Finding:** `_tenantContext` is PRIVATE. No public property named `TenantContext` exists.

**Classification:** Test is accessing private/non-existent API.

**Phase 4 Involvement:** This DbContext design was established in Phase 3 (Tenant Management). Phase 4 did not change it. NOT a Phase 4 regression.

---

#### Test 5: IQueryable.Add() Method

**Current EF Core Pattern:**
```csharp
_dbContext.Users.Add(user);  // Correct - DbSet<User> has .Add()
```

**Test Code Attempting:**
```csharp
_dbContext.Users.Add(user);  // But Users returns IQueryable<User>
```

**Finding:** `DbSet<User>` properly supports `.Add()`. But the test factory might be returning `IQueryable` instead.

**Need to Check:** InMemoryDbContextFactory implementation.

---

## Section 5: Missing APIs - Detailed Evidence

### API #1: User.CreateTenantUser()

**Current Implementation:**
```csharp
public static User CreateTenantUser(Guid tenantId, string email, 
    string passwordHash, string firstName, string lastName)
{
    if (tenantId == Guid.Empty) 
        throw new ArgumentException("TenantId is required.", nameof(tenantId));
    return Create(tenantId, email, passwordHash, firstName, lastName);
}
```

**Status:** ✅ EXISTS - Implemented in Phase 2

**Classification:** Method IS available. Test should work IF called correctly.

---

### API #2: User.MarkEmailVerified()

**Current Implementation:**
```csharp
public void MarkEmailVerified() => IsEmailVerified = true;
```

**Test Calling:**
```csharp
user.VerifyEmail();  // WRONG - method name is MarkEmailVerified
```

**Status:** ✅ EXISTS with DIFFERENT NAME

**Classification:** Test has wrong method name (typo in test, not missing in code).

---

### API #3: AuthTokenResponse Structure

**Current Implementation:**
```csharp
public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int    ExpiresInSeconds,
    UserProfileResponse User  // ← User profile, not UserId
);
```

**Test Accessing:**
```csharp
result.UserId  // WRONG - property doesn't exist
result.User.Id  // CORRECT - should access like this
```

**Status:** ✅ DESIGN IS CORRECT

**Classification:** Test has incorrect property access (test error, not code defect).

---

### API #4: DbContext.TenantContext Property

**Current DbContext:**
```csharp
private readonly ITenantContext _tenantContext;  // PRIVATE

public DbSet<Tenant> TenantSet => Set<Tenant>();
public IQueryable<Tenant> Tenants => TenantSet;
```

**Test Accessing:**
```csharp
_dbContext.TenantContext.TenantId  // Trying to access PRIVATE field
```

**Status:** ❌ NO PUBLIC PROPERTY EXISTS

**Classification:** Test needs to be fixed. DbContext design doesn't expose TenantContext publicly.

**Note:** This IS a legitimate design issue - tests can't access tenant context to set up test data.

---

## Section 6: Root Cause Determination

### Summary of Findings

| Issue | Location | Type | Phase | Fix Needed |
|-------|----------|------|-------|-----------|
| Missing ILogger param | LoginCommandHandlerTests | Test Error | N/A | Fix test constructor |
| Wrong method name (VerifyEmail vs MarkEmailVerified) | VerifyEmailCommandHandlerTests | Test Error | N/A | Rename method call in test |
| Wrong property (UserId vs User) | LoginCommandHandlerTests | Test Error | N/A | Use correct property |
| Private TenantContext access | All auth tests | Design Issue | Phase 2/3 | Add public property or test helper |
| IQueryable vs DbSet | Test infrastructure | Test Setup Error | N/A | Fix InMemoryDbContextFactory |

---

## Section 7: Test Ownership - DOCUMENTED ORIGIN

### Where are these tests documented as belonging?

**Search Results:**

File: `PHASE-2-3-COMPLETION-ROADMAP.md`

```
## Phase 2 Tests
- **Domain Tests:** 38 tests (User, RefreshToken, Tokens)
- **Validator Tests:** 47+ tests (all 9 validators)
- **Infrastructure Tests:** 12 tests (PasswordHasher, TokenService)
- **Total:** 95+ tests
- **Framework:** All configured, ready to execute
- **Compilation:** Minor mocking pattern issues in test helpers 
  (not production code)
```

**Finding:** These tests are explicitly documented as **Phase 2 tests**.

**Conclusion:** Tests belong to Phase 2 (Authentication), not Phase 5 (future).

---

## Section 8: Why Do Tests Exist Before Full Implementation?

### Question: Why are Phase 2 handler tests in the repository?

**Documentation Evidence:**

From `PHASE-2-3-COMPLETION-ROADMAP.md`:

```
### ⏳ PENDING (Test Binding & Integration)

**Handler Test Fixes Needed**

Current Issue: Tests use concrete `KromicStoreDbContext` but 
handlers expect `IApplicationDbContext` interface.

Solution:

Replace all handler tests with this corrected pattern:

// WRONG (current):
private readonly KromicStoreDbContext _dbContext;
_sut = new RegisterCommandHandler(_dbContext, ...);

// CORRECT (use interface):
private readonly IApplicationDbContext _dbContext;
_dbContext = Substitute.For<IApplicationDbContext>();
_sut = new RegisterCommandHandler(_dbContext, ...);
```

**Finding:** Tests exist as a TEST-FIRST / TDD approach. They were written before handlers were wired up correctly, hence the mocking pattern issues.

**Rationale:** This is standard TDD - write tests that describe expected behavior, then implement code to pass tests. The tests are part of Phase 2's development process.

**Why They Don't Compile:** The integration of handlers with the test mocking framework wasn't completed in Phase 2 (roadmap shows this as "pending").

---

## Section 9: Is This a Phase 4 Problem?

### Does Phase 4 implementation explain these errors?

**Analysis:**

Phase 4 scope: Product Catalog (Categories, Products, Collections, Variants)

Phase 4 code does NOT:
- ❌ Modify Authentication handlers
- ❌ Modify User entity
- ❌ Change AuthTokenResponse DTO
- ❌ Change DbContext structure

**All failing tests reference Phase 2 code that Phase 4 never touched.**

**Conclusion:** Phase 4 did NOT break these tests. Tests have pre-existing issues dating back to Phase 2.

---

## Section 10: What Needs To Happen?

### To Make These Tests Compile

These are NOT blocking issues for Phase 4. They are Phase 2 test issues that were documented as "pending" when Phase 2 was marked complete.

**Required Fixes (In Phase 2, not Phase 4):**

1. **Update test constructors** - Add missing `ILogger<T>` parameters
2. **Fix method names** - Change `user.VerifyEmail()` to `user.MarkEmailVerified()`
3. **Fix property access** - Change `result.UserId` to `result.User.Id`
4. **Fix DbContext access** - Add test helper to access TenantContext, OR use Substitute.For<IApplicationDbContext>
5. **Fix EF Core mocking** - Ensure InMemoryDbContextFactory properly returns DbSet<T>, not IQueryable<T>

**These are test maintenance tasks, not Phase 4 regressions.**

---

## Section 11: Build Strategy Decision Point

### Question: Should Phase 4 be blocked by Phase 2 test compilation issues?

**Arguments for YES (block Phase 4):**
- Solution should compile completely
- Tests are in the repository  
- Principle: no compilation errors

**Arguments for NO (don't block Phase 4):**
- Tests don't test Phase 4 code
- Tests have known issues from Phase 2 (documented as "pending")
- Phase 4 scope is isolated (Product Catalog)
- Fixing Phase 2 tests is Phase 2 work, not Phase 4
- Phase 4 implementation is complete and independent

---

## Recommended Action

**DO NOT exclude test files** (that would hide the problem).

**DO investigate and report** which tests belong to which phase and what their status is.

**DO complete Phase 2 test fixes** as part of Phase 2 acceptance (or phase them into Phase 4).

**Then Phase 4 can proceed** with proper understanding of test scope.

---

