# Storefront & Subdomain — Frontend Integration Guide

## Contents

1. [How the Storefront Works](#1-how-the-storefront-works)
2. [Storefront API Reference](#2-storefront-api-reference)
3. [Theme Loading](#3-theme-loading)
4. [Subdomain Update (Admin Panel)](#4-subdomain-update-admin-panel)
5. [Availability Check (Reuse from Registration)](#5-availability-check)

---

## 1. How the Storefront Works

```
Customer visits  →  https://mystore.kromic.in
                              ↓
                 TenantResolutionMiddleware
                 reads Host header: "mystore.kromic.in"
                 extracts subdomain: "mystore"
                 looks up tenant in DB
                 sets TenantContext.TenantId
                              ↓
                 All EF queries auto-filtered to mystore's data
                              ↓
                 StorefrontController returns mystore's products/theme/etc.
```

**Key rule:** All storefront endpoints are `[AllowAnonymous]`. No JWT required.  
Tenant is resolved purely from the **Host header** (the subdomain in the URL).  
If the subdomain doesn't exist in the DB, products/categories return empty arrays — not an error.

---

## 2. Storefront API Reference

**Base URL:** `https://<subdomain>.kromic.in/api/v1/storefront`  
**Auth:** None required on any of these endpoints.

### Store Info
```
GET /api/v1/storefront/info
```
Returns store name, currency, whether it's published.

```json
{
  "tenantId": "ed112d5a-...",
  "storeName": "Rajeesh's Store",
  "description": null,
  "logoUrl": null,
  "currencyCode": "USD",
  "isPublished": true
}
```

Call this first on load. If `isPublished = false`, show a "Store coming soon" page instead of the full storefront.

---

### Theme
```
GET /api/v1/storefront/theme
```
Returns the active published theme. **Call this on every storefront page load** to get branding config.

**Response — theme found:**
```json
{
  "themeId": "3fa85f64-...",
  "name": "Minimal Dark",
  "slug": "minimal-dark",
  "description": "Clean, dark-mode storefront theme",
  "previewImageUrl": "https://cdn.kromic.in/themes/minimal-dark.jpg",
  "isPublished": true
}
```

**Response — no published theme (404):**
```json
{ "message": "No published theme found for this store." }
```

**Storefront load sequence:**
```
1. GET /storefront/info       → check isPublished
2. GET /storefront/theme      → load theme slug/branding
3. GET /storefront/featured-products → populate hero section
4. GET /storefront/categories → populate navigation
```

If step 2 returns 404, fall back to a default theme built into the frontend.

---

### Products
```
GET /api/v1/storefront/products?categoryId=<guid>&skip=0&take=20
GET /api/v1/storefront/products/{id}
GET /api/v1/storefront/featured-products?take=12
GET /api/v1/storefront/search?query=iphone&skip=0&take=20
```

Products are already scoped to the tenant's store. No filter needed from the frontend.

---

### Categories
```
GET /api/v1/storefront/categories?skip=0&take=20
```

---

### Coupon Apply (at checkout)
```
POST /api/v1/storefront/coupons/{couponCode}/apply
```
No auth, no body. Call before order placement to validate and get discount amount.

**Response — valid:**
```json
{
  "message": "Coupon applied.",
  "discountAmount": 150.00,
  "code": "SAVE150"
}
```

**Response — invalid (400):**
```json
{
  "message": "Coupon code is invalid or expired.",
  "code": "BADCODE"
}
```

---

### Active Campaigns
```
GET /api/v1/storefront/campaigns
```
Returns active promotional campaigns for the store. Use for banners, promo strips.  
Currently returns `[]` until campaign backend is wired. Safe to call now.

---

## 3. Theme Loading

### Why the storefront was blank

When you published a theme from the admin panel, it marked the theme as `IsPublished = true` in the `Themes` table. But the storefront frontend wasn't calling the theme endpoint — it was either:
- Not calling `GET /storefront/theme` at all, or
- Calling it at the old path (`/api/v1/themes` which is an admin-only endpoint requiring auth)

### What to implement in the storefront frontend

```typescript
// On storefront app init (e.g. in _app.tsx or root layout)
async function initStorefront() {
  const baseUrl = window.location.origin  // e.g. https://mystore.kromic.in

  // 1. Check if store is live
  const infoRes = await fetch(`${baseUrl}/api/v1/storefront/info`)
  const info = await infoRes.json()

  if (!info.isPublished) {
    showComingSoonPage()
    return
  }

  // 2. Load active theme
  const themeRes = await fetch(`${baseUrl}/api/v1/storefront/theme`)

  if (themeRes.ok) {
    const theme = await themeRes.json()
    applyTheme(theme.slug)         // e.g. load CSS variables, fonts, layout
    document.title = info.storeName
  } else {
    applyTheme('default')          // fallback theme
  }
}

function applyTheme(slug: string) {
  // e.g. dynamically import theme CSS or set CSS custom properties
  document.documentElement.setAttribute('data-theme', slug)
}
```

### Theme response fields and what to do with them

| Field | Use |
|---|---|
| `themeId` | Track which theme is active; useful for analytics |
| `name` | Display in admin panel "Active theme: Minimal Dark" |
| `slug` | Key to look up your frontend theme config (CSS vars, layout) |
| `previewImageUrl` | Show thumbnail in admin panel theme picker |
| `isPublished` | Always `true` from this endpoint (filtered server-side) |

---

## 4. Subdomain Update (Admin Panel)

TenantAdmin can change their store's subdomain from the admin panel settings.

### Availability check (same as registration)
Before showing the save button, check availability in real time:

```
GET /api/v1/auth/check-subdomain?subdomain=newname
Authorization: Bearer <access_token>
```

Response: `{ "available": true, "subdomain": "newname", "previewUrl": "https://newname.kromic.in" }`

Debounce this call at 400ms.

---

### Update the subdomain

```
PATCH /api/v1/tenant/dashboard/subdomain
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request body:**
```json
{ "newSubdomain": "newname" }
```

**Response 200 — success:**
```json
{
  "tenantId": "ed112d5a-...",
  "subdomain": "newname",
  "storeUrl": "https://newname.kromic.in"
}
```

**Response 409 — taken:**
```json
{
  "type": "...",
  "title": "Conflict",
  "status": 409,
  "detail": "The subdomain 'newname' is already taken."
}
```

**Response 400 — invalid format:**
```json
{
  "errors": {
    "NewSubdomain": ["Subdomain can only contain lowercase letters, numbers, and hyphens."]
  }
}
```

---

### UI flow for subdomain update

```
Settings → Store URL

Current URL:  [ mystore          ] .kromic.in   (read-only display)

New subdomain: [ ______________ ] .kromic.in
               ✅ available — https://newname.kromic.in
               ❌ taken
               ❌ invalid format

[  Save New Subdomain  ]  ← disabled until available + confirmed

⚠️  After saving, your store URL will change to https://newname.kromic.in.
    Update any bookmarks or external links. Your old URL will stop working.
```

**After successful save:**
1. Display the new URL with a copy button
2. Show a warning: "Your store URL has changed. Customers must use the new URL."
3. Update the JWT — call `POST /api/v1/auth/refresh` to get a new token. The `tenantId` doesn't change so this is only needed if you display the subdomain from the token. In practice the subdomain is stored in the tenant record, not the JWT, so refresh isn't strictly required.
4. Optionally redirect the admin panel to reload with the new subdomain context.

---

## 5. Availability Check

The check-subdomain endpoint is shared between registration and settings.

```
GET /api/v1/auth/check-subdomain?subdomain=<value>
Authorization: Not required (public endpoint)
```

**Validation rules enforced server-side:**
- 3–63 characters
- Only lowercase letters `a-z`, digits `0-9`, hyphens `-`
- Cannot start or end with a hyphen
- Not a reserved word: `store`, `storeapi`, `admin`, `api`, `auth`, `docs`, `health`, `status`, `cdn`, `assets`
- Not already registered by another tenant

**Client-side pre-validation (run before API call):**
```typescript
function isValidSubdomainFormat(value: string): string | null {
  if (value.length < 3) return 'Must be at least 3 characters'
  if (value.length > 63) return 'Cannot exceed 63 characters'
  if (!/^[a-z0-9][a-z0-9-]*[a-z0-9]$/.test(value))
    return 'Use only lowercase letters, numbers, and hyphens. Cannot start or end with a hyphen.'
  return null  // valid format
}
```

Only call the API if `isValidSubdomainFormat` returns `null`.

---

## Summary of New/Changed Endpoints

| Endpoint | Method | Auth | Purpose |
|---|---|---|---|
| `/api/v1/storefront/theme` | GET | None | Get active published theme |
| `/api/v1/storefront/campaigns` | GET | None | Get active campaigns (customer) |
| `/api/v1/storefront/coupons/{code}/apply` | POST | None | Validate coupon at checkout |
| `/api/v1/tenant/dashboard/subdomain` | PATCH | TenantAdmin | Change store subdomain |
| `/api/v1/auth/check-subdomain?subdomain=x` | GET | None | Check availability (registration + settings) |
