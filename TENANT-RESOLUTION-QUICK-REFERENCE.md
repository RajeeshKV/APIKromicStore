# Tenant Resolution - Quick Reference Guide

## For Developers

### How Tenant Resolution Works

When a request comes in, the middleware tries these in order:

```
1. Custom Domain? → Use it
   ↓ (if not found)
2. Subdomain? → Use it
   ↓ (if not found)
3. JWT tenantId? → Verify in DB, then use it
   ↓ (if not found or fails)
4. Dev Header? → Use it (dev only)
   ↓ (if not found)
5. No tenant → Request continues (tenant context may be null)
```

---

## For Tenant Admins / API Users

### Making API Calls (After Login)

1. **Login**
   ```bash
   curl -X POST "https://storeapi.kromic.in/api/v1/auth/login" \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@example.com","password":"password"}'
   ```

2. **Get Token from Response**
   ```json
   {
     "accessToken": "eyJhbGc...",
     "refreshToken": "...",
     "user": {...}
   }
   ```

3. **Use Token to Call Endpoints** (tenant is auto-resolved from JWT)
   ```bash
   curl -X POST "https://storeapi.kromic.in/api/v1/categories" \
     -H "Authorization: Bearer eyJhbGc..." \
     -H "Content-Type: application/json" \
     -d '{"name":"Electronics"}'
   ```

**That's it!** The middleware automatically resolves your tenant from the JWT token.

---

### Common Scenarios

#### ✅ Has Custom Domain?
```
storeapi.kromic.in → Resolves via Custom Domain ✅
```

#### ✅ Has Subdomain?
```
mystoresubdomain.kromic.in → Resolves via Subdomain ✅
```

#### ✅ No Domain Setup Yet?
```
storeapi.kromic.in + JWT token → Resolves via JWT (with DB verification) ✅
```

#### ✅ Local Development?
```
localhost:5000 + X-Kromic-TenantId header → Resolves via Dev Header ✅
```

---

## For DevOps / Site Reliability

### Security Checklist

- ✅ JWT includes `allowTenantIdBypass` claim? (Yes, added by TokenService)
- ✅ Middleware verifies user-tenant in DB? (Yes, critical check)
- ✅ Tenant status validated? (Yes, IsActive() check)
- ✅ Tampering logged? (Yes, LogWarning for mismatches)
- ✅ No hardcoded bypasses? (Correct, no bypasses)

### Monitoring Alerts to Set Up

1. **Security Alert - Possible Token Tampering**
   ```
   Search logs for: "Security: User {UserId} attempted to access tenant"
   → Investigate immediately
   ```

2. **Tenant Access Denied**
   ```
   Search logs for: "Tenant {TenantId} not found or inactive"
   → Check if tenant is suspended/deleted
   ```

3. **Database Verification Failed**
   ```
   Search logs for: "but has no relationship in database"
   → Possible attack or data inconsistency
   ```

---

## For Security Team

### Risk Assessment

| Aspect | Risk Level | Mitigation |
|--------|-----------|-----------|
| JWT Forgery | LOW | Database verification required |
| Token Scope Creep | LOW | Explicit bypass flag + DB check |
| Unauthorized Access | LOW | User-tenant relationship verified |
| Cross-Tenant Access | LOW | Database query enforces TenantId match |
| Inactive Tenant Access | LOW | Status.IsActive() check |

**Overall Risk:** 🟢 LOW (Production-Safe)

### Implementation Review

✅ **3-Layer Defense:**
1. Explicit bypass flag in JWT
2. Database verification of user-tenant relationship
3. Active tenant status validation

✅ **No Single Points of Failure:**
- JWT alone cannot grant access
- Tenant status can be revoked immediately
- All attempts logged

✅ **Standard Multi-Tenant Pattern:**
- Used in Stripe, Salesforce, etc.
- Database verification is industry standard

---

## For QA / Testers

### Testing Tenant Resolution

#### Test Case 1: JWT Fallback Works
```bash
1. Create user "testadmin@example.com" with Tenant A
2. No subdomain setup for Tenant A
3. Login → Get JWT with tenantId=Tenant A
4. Call: POST /api/v1/categories
   Authorization: Bearer <JWT_TOKEN>
5. Expected: 200 OK, category created ✅
```

#### Test Case 2: Tampering Detected
```bash
1. Get valid JWT for User in Tenant A
2. Manually modify JWT to tenantId=Tenant B (if possible)
3. Call: POST /api/v1/categories
   Authorization: Bearer <MODIFIED_JWT_TOKEN>
4. Expected: 403 Forbidden ✅
5. Check logs: Security warning logged ✅
```

#### Test Case 3: Inactive Tenant Blocked
```bash
1. Create user in Tenant C
2. Set Tenant C status to "Inactive"
3. Login → Get JWT
4. Call: POST /api/v1/categories
   Authorization: Bearer <JWT_TOKEN>
5. Expected: 403 Forbidden ✅
6. Check logs: Tenant inactive warning ✅
```

#### Test Case 4: Development Header Works
```bash
1. Set header: X-Kromic-TenantId: <TENANT_ID>
2. Call: POST /api/v1/categories
3. Expected: Works in development mode ✅
4. Expected: Fails in production (IsActive() check) ✅
```

---

## For Support / Customer Success

### User Issue: "Getting 403 on All Endpoints"

**Possible Causes:**
1. ❓ User not authenticated?
   → Ask for login credentials, verify JWT in response

2. ❓ JWT token expired?
   → User needs to login again to get fresh token

3. ❓ User deleted or inactive?
   → Check user record in database

4. ❓ Tenant suspended?
   → Check tenant status in database

5. ❓ User-tenant relationship broken?
   → Check database for inconsistency

### Resolution Steps

```
1. Ask user: "Did you login and get a token?"
   → If no, guide them through login

2. Check JWT token at jwt.io:
   → Verify tenantId claim present
   → Verify exp (expiration) in future

3. Check database:
   SELECT * FROM Users WHERE Email = 'user@example.com';
   → Verify TenantId is not null

4. Check logs:
   → Search for user ID in middleware logs
   → Look for "Security" warnings

5. If all good but still 403:
   → Escalate to engineering team
```

---

## Code Examples

### Getting tenantId from JWT (Frontend)

```javascript
const token = localStorage.getItem('accessToken');
const decoded = jwt_decode(token);
const tenantId = decoded.tenantId;
const isEmailVerified = decoded.isEmailVerified;

if (!isEmailVerified) {
  // Show verification banner
}
```

### Checking Tenant in Database (SQL)

```sql
-- Is user-tenant relationship correct?
SELECT u.Id, u.Email, u.TenantId, t.StoreName, t.Status
FROM Users u
LEFT JOIN Tenants t ON u.TenantId = t.Id
WHERE u.Email = 'user@example.com';
```

### Adding Subdomain to Tenant (SQL)

```sql
-- Add subdomain to tenant
INSERT INTO TenantDomains (TenantId, Subdomain, IsVerified)
VALUES ('TENANT_ID', 'mystore', true);

-- Now tenant can be accessed via: mystore.kromic.in
```

---

## FAQ

**Q: Is JWT fallback secure?**
A: Yes. Database verification is the security layer, not JWT claims alone.

**Q: Can I disable JWT fallback?**
A: Yes, modify middleware to skip `ResolveTenantFromJwtWithValidationAsync` method.

**Q: What if JWT secret leaks?**
A: Rotate the secret, all old tokens become invalid, users re-authenticate.

**Q: Can a user access multiple tenants?**
A: No, JWT contains tenantId for one tenant. User's database record has one TenantId.

**Q: What happens if a tenant is suspended?**
A: Middleware checks `Status.IsActive()` and denies access immediately.

**Q: How do I set up custom domain?**
A: Add to TenantDomains table with CustomDomain and IsVerified=true.

**Q: Can I use this in production?**
A: Yes, this is production-grade. All tests passing, security verified.

---

**Last Updated:** 2026-07-31  
**Status:** ✅ Production Ready
