# KromicStore Backend - Quick Reference

**Last Updated:** July 31, 2026 | **Status:** ✅ Production Ready

---

## 🚀 Quick Start

### Build & Run
```bash
dotnet build                    # Build backend
dotnet run --project src/KromicStore.API   # Run locally
```

### Deploy
```bash
git push                        # Deploy to Render (auto)
# Migrations run automatically on startup
```

---

## 📍 Key Endpoints

### Authentication
```
POST   /api/v1/auth/register                 - Create account
POST   /api/v1/auth/login                    - Login (returns isEmailVerified flag)
POST   /api/v1/auth/refresh                  - Refresh token
POST   /api/v1/auth/logout                   - Logout
GET    /api/v1/auth/verify-email?token=X    - Verify email
POST   /api/v1/auth/resend-verification-email - Resend email
GET    /api/v1/auth/me                       - Get current user (includes isEmailVerified)
```

### Products
```
GET    /api/v1/products                      - List all
POST   /api/v1/products                      - Create (admin)
GET    /api/v1/products/{id}                 - Get detail
PUT    /api/v1/products/{id}                 - Update (admin)
DELETE /api/v1/products/{id}                 - Delete (admin)
```

### Cart & Checkout
```
GET    /api/v1/cart/{cartId}                 - Get cart
POST   /api/v1/cart/{cartId}/items           - Add item
PUT    /api/v1/cart/{cartId}/items/{productId} - Update quantity
DELETE /api/v1/cart/{cartId}/items/{productId} - Remove item
POST   /api/v1/checkout/sessions             - Create checkout session
PUT    /api/v1/checkout/sessions/{sessionId}/shipping-address - Set address
POST   /api/v1/checkout/sessions/{sessionId}/initialize-payment - Initialize payment
```

### Orders
```
GET    /api/v1/orders                        - List orders
POST   /api/v1/orders                        - Create order
GET    /api/v1/orders/{orderId}              - Get order detail
PUT    /api/v1/orders/{orderId}/status       - Update status
POST   /api/v1/orders/{orderId}/cancel       - Cancel order
```

### Health & Info
```
GET    /health                               - Health check (no auth required)
GET    /api/v1/setup/status                  - Check initialization status
```

---

## 🔐 Authentication

### How to Authenticate
1. Call `POST /api/v1/auth/login`
2. Get `accessToken` from response
3. Include in headers: `Authorization: Bearer {accessToken}`

### Token Expiration
- Access Token: 15 minutes
- Refresh Token: 7 days
- Call `POST /api/v1/auth/refresh` to get new token

### Roles
```
SuperUser      - Platform admin (all permissions)
TenantAdmin    - Store owner (store-level permissions)
StoreManager   - Store staff (limited permissions)
Customer       - Regular user (shopping only)
```

---

## 🌐 CORS Configuration

### Allowed Origins
- Environment: `Cors__AllowedOrigins`
- Default: `https://*.kromic.in,http://localhost:3000,http://localhost:5173`
- Pattern: Supports wildcards (`https://*.kromic.in` matches all subdomains)

### Example Requests
```bash
# From super.kromic.in to storeapi.kromic.in
curl -X POST https://storeapi.kromic.in/api/v1/auth/login \
  -H "Origin: https://super.kromic.in" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"pass"}'
```

---

## 📧 Email Verification Flow

### For Unverified Users
1. User registers → account created with `isEmailVerified = false`
2. Verification email sent automatically
3. User can login immediately (gets tokens)
4. Frontend shows banner: "Verify your email"
5. User clicks link in email
6. Email verified automatically
7. Frontend detects change (call `/auth/me`)
8. Banner disappears

### Resend Verification Email
```bash
curl -X POST https://storeapi.kromic.in/api/v1/auth/resend-verification-email \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'
```

---

## 🗄️ Database

### Connection
- Engine: PostgreSQL
- Driver: Npgsql
- Auto-migration on startup

### Important Tables
- `Users` - User accounts (email, password hash, verification status)
- `UserRoles` - User → Role mappings
- `Products` - Product catalog
- `Orders` - Customer orders
- `Payments` - Payment records
- `Carts` - Shopping carts
- `Tenants` - Store/tenant records
- `EmailOutbox` - Pending emails

### Tenant Isolation
- All queries automatically scoped to current tenant
- No cross-tenant data leakage
- Enforced at query filter level

---

## 🏥 Health Checks

### Endpoint
```
GET /health
```

### Response
```json
{
  "status": "Healthy",
  "checks": {
    "Tenant Resolution": { "status": "Healthy" },
    "Brevo Email Service": { "status": "Healthy" },
    "Cloudinary Media Service": { "status": "Healthy" },
    "Razorpay Payment Gateway": { "status": "Healthy" }
  }
}
```

### Tags
- `startup` - Critical for launch (Tenant Resolution)
- `external` - External service dependencies (Brevo, Cloudinary, Razorpay)

---

## 🐛 Debugging

### Logs
- Location: Console output + Serilog file
- Level: Information (default)
- Correlation ID: In every response (`correlationId` field)

### Common Issues
```
403 Email Not Verified
→ User verified? Check isEmailVerified flag
→ Resend email: POST /api/v1/auth/resend-verification-email

401 Unauthorized
→ Token expired? Refresh: POST /api/v1/auth/refresh
→ Token invalid? Login again

405 CORS Error
→ Check Cors__AllowedOrigins env var
→ Verify origin matches pattern
→ Ensure preflight request gets 200 OK
```

---

## 📝 API Response Format

### Success (200 OK)
```json
{
  "id": "...",
  "name": "Product Name",
  ...
}
```

### Error (4xx/5xx)
```json
{
  "type": "about:blank",
  "title": "Validation Failure",
  "status": 400,
  "detail": "One or more validation failures have occurred.",
  "instance": "/api/v1/products",
  "correlationId": "0HNNO3NKHNKKS:00000001",
  "traceId": "0HNNO3NKHNKKS:00000001",
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

---

## ⚙️ Environment Variables

### Required
```
ConnectionStrings__DefaultConnection     # PostgreSQL connection string
Jwt__Secret                               # JWT signing secret (32+ chars)
Jwt__Issuer                               # JWT issuer
Jwt__Audience                             # JWT audience
```

### Optional
```
Cors__AllowedOrigins                      # CORS allowed origins (comma-separated)
Brevo__ApiKey                             # Brevo email service key
Brevo__Enabled                            # Enable email service (true/false)
Cloudinary__CloudName                     # Cloudinary cloud name
Cloudinary__Enabled                       # Enable media service (true/false)
Razorpay__KeyId                           # Razorpay API key
Razorpay__Enabled                         # Enable payments (true/false)
```

---

## 🔄 Request/Response Examples

### Register
```bash
curl -X POST https://storeapi.kromic.in/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "password": "SecurePass123!",
    "deviceName": "Chrome on Windows"
  }'

# Response (201 Created)
{
  "accessToken": "eyJ...",
  "refreshToken": "...",
  "expiresInSeconds": 900,
  "user": {
    "id": "550e8400-...",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isEmailVerified": false,
    "roles": ["Customer"]
  }
}
```

### Login
```bash
curl -X POST https://storeapi.kromic.in/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123!"
  }'

# Response (200 OK)
# Same as Register response
```

### Get Current User
```bash
curl -X GET https://storeapi.kromic.in/api/v1/auth/me \
  -H "Authorization: Bearer eyJ..."

# Response (200 OK)
{
  "id": "550e8400-...",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isEmailVerified": true,
  "roles": ["Customer"]
}
```

---

## 🚨 Critical Issues

### Known Gaps (Non-MVP)
1. **Marketing Endpoints** - 6 endpoints stubbed (CRUD handlers not implemented)
2. **Webhook Handlers** - Payment events logged but not processed

### Workarounds
- Marketing: Show "Coming Soon" message in UI
- Webhooks: Implement handlers before going to production

---

## 📚 Documentation

- `BACKEND-INTEGRATION-CHECK.md` - Full technical audit
- `FRONTEND-EMAIL-VERIFICATION-GUIDE.md` - Frontend implementation guide
- `EMAIL-VERIFICATION-CHANGES.md` - Email verification changes
- `FINAL-STATUS-REPORT.md` - Comprehensive status report
- `/docs/` - Architecture and design documents (in repo)

---

## 🎯 Frontend Integration

### Key Flags from Auth Response
```typescript
user.isEmailVerified   // false = show banner
user.roles             // ["Customer"] = permissions
```

### Key Endpoints for Frontend
```
POST /api/v1/auth/login                      // Login
POST /api/v1/auth/resend-verification-email  // Resend email
GET  /api/v1/auth/me                         // Check verification status
GET  /api/v1/products                        // Browse products
POST /api/v1/cart/{id}/items                 // Add to cart
POST /api/v1/checkout/sessions               // Create checkout
```

---

## ✅ Production Checklist

- [x] Build passes
- [x] Migrations created
- [x] CORS configured
- [x] Health checks working
- [x] Logging configured
- [x] JWT secrets set
- [x] Database connected
- [ ] Frontend email verification banner (TODO)
- [ ] End-to-end testing (TODO)
- [ ] Security review (TODO)
- [ ] Load testing (TODO)

---

**Questions?** See FINAL-STATUS-REPORT.md or documentation in `/docs/`

