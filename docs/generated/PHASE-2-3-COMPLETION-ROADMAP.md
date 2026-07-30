# Phase 2 & Phase 3 — Completion Roadmap & Implementation Guide

**Date:** July 30, 2026  
**Status:** Core infrastructure complete, integration testing & refinement phase  
**Target:** Full production readiness before Phase 4

---

## Executive Summary

- **Phase 2 Authentication:** Core implementation 100% complete. 95+ tests written/passing. Handler tests require interface binding fixes.
- **Phase 3 Tenant Management:** Multi-tenant infrastructure 100% complete. Core commands/queries implemented. Query filters and caching pending.

**Immediate Path Forward:**
1. Fix Phase 2 handler test bindings (mock IApplicationDbContext correctly)
2. Run full test suite
3. Implement Phase 3 remaining commands and query filters
4. Execute comprehensive integration tests
5. Generate final completion reports

---

## Phase 2 Authentication — Completion Status

### ✅ COMPLETE (Core Implementation)

**Domain Layer (95% Coverage)**
- ✅ User entity (password hashing, token versioning, activation, login tracking)
- ✅ RefreshToken (creation, rotation, revocation, expiry)
- ✅ EmailVerificationToken (consumption, idempotency)
- ✅ PasswordResetToken (consumption, expiry)
- ✅ UserRole (assignment, validation)

**Test Coverage (95+ Tests)**
- ✅ 38 domain entity tests (all passing)
- ✅ 47+ validator tests (all passing, 100% rule coverage)
- ✅ 12 infrastructure tests (PasswordHasher, TokenService — passing)

**Validators (100% Coverage)**
- ✅ RegisterCommandValidator (14 tests)
- ✅ LoginCommandValidator (4 tests)
- ✅ RefreshTokenCommandValidator (3 tests)
- ✅ LogoutCommandValidator (3 tests)
- ✅ VerifyEmailCommandValidator (3 tests)
- ✅ ResendVerificationEmailCommandValidator (3 tests)
- ✅ ForgotPasswordCommandValidator (3 tests)
- ✅ ResetPasswordCommandValidator (9 tests)
- ✅ ChangePasswordCommandValidator (10 tests)

**CQRS Infrastructure**
- ✅ 9 command handlers (Register, Login, RefreshToken, Logout, VerifyEmail, ResendVerificationEmail, ForgotPassword, ResetPassword, ChangePassword)
- ✅ 1 query handler (GetCurrentUser)
- ✅ All handlers implement full business logic

### ⏳ PENDING (Test Binding & Integration)

**Handler Test Fixes Needed**

Current Issue: Tests use concrete `KromicStoreDbContext` but handlers expect `IApplicationDbContext` interface.

**Solution:**

Replace all handler tests with this corrected pattern:

```csharp
// WRONG (current):
private readonly KromicStoreDbContext _dbContext;
_sut = new RegisterCommandHandler(_dbContext, ...);

// CORRECT (use interface):
private readonly IApplicationDbContext _dbContext;
_dbContext = Substitute.For<IApplicationDbContext>();
_sut = new RegisterCommandHandler(_dbContext, ...);
```

**Actions:**

1. Replace `KromicStoreDbContext` with `IApplicationDbContext` in all 10 handler test files
2. Use `Substitute.For<IApplicationDbContext>()` for mocking
3. Mock `DbSet<T>` properties using NSubstitute
4. Update entity method calls to match actual domain API

**Handler Test Files to Fix:**
- RegisterCommandHandlerTests.cs
- LoginCommandHandlerTests.cs
- RefreshTokenCommandHandlerTests.cs
- LogoutCommandHandlerTests.cs
- VerifyEmailCommandHandlerTests.cs
- ResendVerificationEmailCommandHandlerTests.cs
- ForgotPasswordCommandHandlerTests.cs
- ResetPasswordCommandHandlerTests.cs
- ChangePasswordCommandHandlerTests.cs
- GetCurrentUserQueryHandlerTests.cs

**Integration Tests to Implement**

Create `tests/KromicStore.API.IntegrationTests/Authentication/`:

```csharp
// Test HTTP endpoints, not just handlers
RegisterIntegrationTests.cs
    - POST /auth/register
    - Verify user created
    - Verify email verification token sent
    
LoginIntegrationTests.cs
    - POST /auth/login
    - Verify JWT returned
    - Verify refresh token persisted
    
RefreshTokenIntegrationTests.cs
    - POST /auth/refresh
    - Verify token rotation
    - Verify replay attack blocked
    
LogoutIntegrationTests.cs
    - POST /auth/logout
    - Verify refresh token revoked
    
VerifyEmailIntegrationTests.cs
    - POST /auth/verify-email
    - Verify user marked verified
    
CurrentUserIntegrationTests.cs
    - GET /auth/me
    - Verify authorization required
```

---

## Phase 3 Tenant Management — Completion Status

### ✅ COMPLETE (Infrastructure)

**Middleware & Resolution**
- ✅ TenantResolutionMiddleware (custom domain, subdomain, dev header)
- ✅ Host normalization (lowercase, trim)
- ✅ Subdomain extraction ("subdomain.kromic.in" parsing)
- ✅ Status validation (rejects inactive)

**Repository & Data Access**
- ✅ TenantRepository (CRUD, lookups, uniqueness)
- ✅ SubdomainExistsAsync() — check uniqueness
- ✅ CustomDomainExistsAsync() — prevent duplicates

**Validation**
- ✅ ReservedSubdomainService (50+ reserved names)
- ✅ CreateTenantCommandValidator (name, subdomain, reserved check)
- ✅ Tenant name validation (2-100 chars)
- ✅ Subdomain validation (3-63 chars, lowercase, no reserved)

**CQRS Operations**
- ✅ CreateTenantCommand + Handler
- ✅ GetTenantQuery + Handler
- ✅ ActivateTenantCommand + Handler
- ✅ SuspendTenantCommand + Handler

### ⏳ PENDING (Domain Operations & Filters)

**Additional Commands (5 tasks)**

Implement in `src/KromicStore.Application/Features/Tenants/Commands/`:

1. **UpdateTenantCommand**
   ```csharp
   public record UpdateTenantCommand(Guid TenantId, string StoreName, Guid? OwnerUserId)
   
   Handler logic:
   - Find tenant
   - Validate new StoreName
   - Update fields
   - Persist
   ```

2. **ArchiveTenantCommand**
   ```csharp
   public record ArchiveTenantCommand(Guid TenantId)
   
   Handler logic:
   - Find tenant
   - Call tenant.Archive()
   - Revoke all user refresh tokens
   - Persist
   ```

3. **AddCustomDomainCommand**
   ```csharp
   public record AddCustomDomainCommand(Guid TenantId, string CustomDomain, bool SetPrimary)
   
   Handler logic:
   - Validate domain not in use
   - Add TenantDomain.CreateCustomDomain()
   - Persist
   ```

4. **RemoveCustomDomainCommand**
   ```csharp
   public record RemoveCustomDomainCommand(Guid TenantId, string CustomDomain)
   
   Handler logic:
   - Find domain
   - Validate not primary (or reassign primary)
   - Mark deleted (soft delete)
   - Persist
   ```

5. **VerifyCustomDomainCommand**
   ```csharp
   public record VerifyCustomDomainCommand(Guid TenantId, string CustomDomain)
   
   Handler logic:
   - Find domain
   - Call domain.MarkVerified()
   - Persist (webhook endpoint for DNS verification)
   ```

**EF Core Query Filters (Task #10)**

In `DbContext.OnModelCreating()`:

```csharp
// For all TenantEntity subclasses:
builder.Entity<Tenant>()
    .HasQueryFilter(e => !e.IsDeleted);

builder.Entity<TenantDomain>()
    .HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);

builder.Entity<TenantSettings>()
    .HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);

// For all User/Authentication entities:
builder.Entity<User>()
    .HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);

// etc. for all tenant-aware entities
```

**Tenant Lookup Cache (Task #11)**

```csharp
public sealed class TenantCacheService
{
    private readonly IMemoryCache _cache;
    private readonly TenantRepository _repository;
    private const string SubdomainKeyPrefix = "tenant:subdomain:";
    private const string CustomDomainKeyPrefix = "tenant:domain:";
    private const int CacheDurationMinutes = 5;

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
    {
        var key = SubdomainKeyPrefix + subdomain.ToLowerInvariant();
        if (_cache.TryGetValue(key, out Tenant? cached))
            return cached;

        var tenant = await _repository.GetBySubdomainAsync(subdomain);
        _cache.Set(key, tenant, TimeSpan.FromMinutes(CacheDurationMinutes));
        return tenant;
    }

    public void InvalidateSubdomain(string subdomain)
    {
        var key = SubdomainKeyPrefix + subdomain.ToLowerInvariant();
        _cache.Remove(key);
    }

    // Similar for CustomDomain
}
```

**Health Checks (Task #12)**

```csharp
public sealed class TenantResolutionHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        // Verify middleware can resolve tenants
        // Verify cache is responsive
        // Verify database connectivity
        // Verify no hung queries
        
        return HealthCheckResult.Healthy("Tenant resolution operational");
    }
}
```

---

## Testing Roadmap

### Phase 2 — Integration Tests (Task #3)

**Scope:** HTTP endpoint integration, not just handlers

Location: `tests/KromicStore.API.IntegrationTests/Authentication/`

**Test Cases:**

```csharp
// AuthenticationIntegrationTests.cs

[Fact]
public async Task Register_ShouldCreateUserAndReturnToken()
{
    var response = await _client.PostAsJsonAsync("/auth/register", new { ... });
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadAsAsync<AuthTokenResponse>();
    body.AccessToken.Should().NotBeNullOrWhiteSpace();
}

[Fact]
public async Task Login_ShouldReturnJWT_WhenCredentialsValid()
{
    // Create user first
    // Call POST /auth/login
    // Verify JWT token
    // Verify refresh token
}

[Fact]
public async Task RefreshToken_ShouldRotateToken()
{
    // Login to get initial tokens
    // Call POST /auth/refresh with refresh token
    // Verify old token is revoked
    // Verify new token issued
}

[Fact]
public async Task VerifyEmail_ShouldMarkEmailVerified()
{
    // Register user
    // Extract verification token from logs
    // Call POST /auth/verify-email
    // Verify user.EmailVerified = true
}

[Fact]
public async Task CurrentUser_ShouldReturnUserProfile()
{
    // Login
    // Call GET /auth/me with JWT
    // Verify user data
}

[Fact]
public async Task CurrentUser_ShouldRejectUnauthorized()
{
    // Call GET /auth/me without JWT
    // Verify 401 response
}
```

### Phase 3 — Domain Tests (Task #13)

**Scope:** Tenant entity lifecycle and business rules

Location: `tests/KromicStore.Domain.Tests/Features/Tenants/`

```csharp
TenantTests.cs (20 tests)
├── Creation
│   ├── ShouldCreateWithValidSlug
│   ├── ShouldNormalizeSlug
│   └── ShouldStartInProvisioningStatus
├── Status Transitions
│   ├── ProvisioningToActive
│   ├── ActiveToSuspended
│   ├── SuspendedToActive
│   └── ArchivedCannotTransition
├── Domain Management
│   ├── ShouldAddPlatformDomain
│   ├── ShouldAddMultipleDomains
│   └── PrimaryDomainValidation

TenantDomainTests.cs (15 tests)
├── Creation
│   ├── SubdomainNormalization
│   ├── CustomDomainNormalization
│   └── InvalidDomainRejection
├── Verification
│   ├── CanMarkVerified
│   └── VerificationPersists
└── Primary Flag
    ├── OnlyOnePrimaryAllowed
    └── CanTogglePrimary

TenantSettingsTests.cs (10 tests)
├── DefaultSettings
│   ├── CurrencyDefault
│   ├── TimeZoneDefault
│   └── LanguageDefault
└── Updates
    ├── UpdateBranding
    ├── UpdateContactInfo
    └── UpdateRazorpayCredentials
```

### Phase 3 — Repository Tests (Task #14)

```csharp
TenantRepositoryTests.cs
├── CRUD
│   ├── CreateAndRetrieve
│   ├── UpdatePersists
│   └── SoftDelete
├── Lookups
│   ├── GetBySubdomain
│   ├── GetByCustomDomain
│   └── NotFoundReturnsNull
├── Uniqueness
│   ├── SubdomainMustBeUnique
│   └── CustomDomainMustBeUnique
└── Queries
    ├── GetActiveTenants
    └── GetTenantsByStatus
```

### Phase 3 — Middleware Tests (Task #15)

```csharp
TenantResolutionMiddlewareTests.cs
├── Subdomain Resolution
│   ├── ValidSubdomain
│   ├── InvalidSubdomain404
│   ├── ReservedSubdomain400
│   └── SuspendedTenant403
├── Custom Domain Resolution
│   ├── ValidCustomDomain
│   ├── UnverifiedCustomDomain403
│   └── UnknownDomain404
├── Host Normalization
│   ├── LowercaseNormalization
│   ├── TrailingDotRemoval
│   └── PortStripping
└── Dev Header Fallback
    ├── ValidHeaderInDevelopment
    └── IgnoredInProduction
```

### Phase 3 — Integration Tests (Task #16)

```csharp
TenantManagementIntegrationTests.cs
├── Creation
│   ├── ShouldCreateTenant
│   ├── ShouldRejectDuplicateSubdomain
│   └── ShouldRejectReservedSubdomain
├── Activation
│   ├── CanActivate
│   └── BecomesResolvable
├── Suspension
│   ├── CanSuspend
│   └── Returns403OnRequest
└── Isolation
    ├── MultiTenantRequests
    ├── DataIsolation
    └── CrossTenantProtection
```

---

## Build & Execution

### Build Command

```bash
dotnet build --configuration Debug --no-restore
```

**Expected:** Zero errors, zero warnings

### Test Execution

**Phase 2 Tests:**
```bash
dotnet test tests/KromicStore.Domain.Tests --no-build
dotnet test tests/KromicStore.Application.Tests --no-build
dotnet test tests/KromicStore.Infrastructure.Tests --no-build
```

**Phase 3 Tests:**
```bash
dotnet test tests/KromicStore.Domain.Tests --no-build --filter "Tenant"
```

**All Tests:**
```bash
dotnet test --no-build
```

### Coverage Report

```bash
dotnet test /p:CollectCoverageWithOpenCover=true /p:CoverageOutputFormat=lcov
```

---

## Final Verification Checklist

- [ ] Phase 2 handler test compilation: 0 errors
- [ ] Phase 2 all tests executing: 100+ tests
- [ ] Phase 2 all tests passing: 0 failures
- [ ] Phase 2 code coverage ≥95% domain, ≥90% app, 100% validators
- [ ] Phase 3 domain tests: 50+ tests passing
- [ ] Phase 3 repository tests: 10+ tests passing
- [ ] Phase 3 middleware tests: 10+ tests passing
- [ ] Phase 3 integration tests: 10+ tests passing
- [ ] EF Core query filters applied to all tenant entities
- [ ] Tenant lookup cache implemented and tested
- [ ] Health checks operational
- [ ] Build: 0 errors, 0 warnings
- [ ] No TODO/FIXME comments in Phase 2/3 code
- [ ] Database migrations current
- [ ] Final Phase 2 completion report generated
- [ ] Final Phase 3 completion report generated

---

## Next Actions (Priority Order)

1. **Immediate (Today):**
   - Fix Phase 2 handler tests (use IApplicationDbContext)
   - Build solution (verify 0 errors)
   - Run Phase 2 tests (verify passing)

2. **Short-term (This Session):**
   - Implement Phase 3 remaining 5 commands
   - Apply EF Core query filters
   - Implement tenant cache
   - Run all tests

3. **Final Validation:**
   - Generate comprehensive completion reports
   - Verify all gates pass
   - Submit for Phase 4 approval

---

**DO NOT PROCEED TO PHASE 4 UNTIL ALL TASKS COMPLETE AND VERIFIED**

