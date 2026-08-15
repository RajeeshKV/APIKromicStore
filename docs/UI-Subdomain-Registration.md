# Subdomain Registration — UI Integration Guide

## Overview

When a new user registers on KromicStore, they choose their own subdomain.
This becomes their store URL: `https://<subdomain>.kromic.in`

The UI must:
1. Show a subdomain input field on the registration form
2. Check availability in real-time as the user types (debounced)
3. Block form submission until a valid, available subdomain is confirmed
4. Pass the chosen subdomain (and optional store name) to the register endpoint

---

## New / Changed Endpoints

### 1. Check Subdomain Availability
**Real-time availability check — call this as the user types (debounced 400ms)**

```
GET /api/v1/auth/check-subdomain?subdomain=<value>
Authorization: None (public)
```

**Response — available:**
```json
{
  "available": true,
  "subdomain": "myshop",
  "reason": null,
  "previewUrl": "https://myshop.kromic.in"
}
```

**Response — taken or invalid:**
```json
{
  "available": false,
  "subdomain": "admin",
  "reason": "This subdomain is reserved by the platform.",
  "previewUrl": null
}
```

Possible `reason` values:
| Reason | Meaning |
|---|---|
| `"This subdomain is reserved by the platform."` | Blocked words: `store`, `admin`, `api`, `auth`, `docs`, `health`, `status`, `cdn`, `assets`, `storeapi` |
| `"This subdomain is already taken."` | Another tenant already uses this slug |
| `"Subdomain must be 3–63 characters."` | Too short or too long |
| `"Use only lowercase letters, numbers, and hyphens. Cannot start or end with a hyphen."` | Invalid characters |

---

### 2. Register
**Changed — now requires `subdomain` and optional `storeName`**

```
POST /api/v1/auth/register
Authorization: None (public)
```

**Request body (updated):**
```json
{
  "firstName": "Rajeesh",
  "lastName": "KV",
  "email": "rajeesh@example.com",
  "password": "SecurePass1!",
  "subdomain": "rajeeshstore",
  "storeName": "Rajeesh's Electronics",
  "deviceName": "Chrome/Windows"
}
```

| Field | Required | Rules |
|---|---|---|
| `firstName` | Yes | 1–100 chars |
| `lastName` | Yes | 1–100 chars |
| `email` | Yes | Valid email, globally unique |
| `password` | Yes | 8+ chars, upper, lower, number, special char |
| `subdomain` | **Yes** | 3–63 chars, `^[a-z0-9][a-z0-9-]*[a-z0-9]$`, not reserved, not taken |
| `storeName` | No | 1–100 chars. Defaults to `"<firstName>'s Store"` if omitted |
| `deviceName` | No | For refresh token tracking |

**Response (same as before):**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "...",
  "expiresInSeconds": 900,
  "user": {
    "id": "...",
    "tenantId": "...",
    "email": "rajeesh@example.com",
    "firstName": "Rajeesh",
    "lastName": "KV",
    "isEmailVerified": false,
    "roles": ["TenantAdmin"]
  }
}
```

**Error responses:**
- `400` — Validation failure (missing field, bad format)
- `409` — Email already registered OR subdomain taken (race condition safety net — check-subdomain endpoint should prevent this in normal flow)

---

## UI Implementation

### Registration Form Fields

```
First Name*        [________________]
Last Name*         [________________]
Email*             [________________]
Password*          [________________]

Store subdomain*   [________________] .kromic.in
                   ✅ myshop.kromic.in is available   ← live feedback
                   — OR —
                   ❌ admin is reserved               ← live feedback
                   ❌ myshop is already taken          ← live feedback

Store Name         [________________]  (optional, defaults to "First's Store")

                   [ Create Store ]   ← disabled until subdomain is confirmed available
```

### Subdomain Availability Check Logic

```typescript
// Debounce the API call — don't fire on every keystroke
const checkSubdomain = debounce(async (value: string) => {
  // Client-side pre-validation before hitting the API
  const normalized = value.toLowerCase().trim()

  if (normalized.length < 3) {
    setSubdomainStatus({ state: 'idle' })
    return
  }

  if (!/^[a-z0-9][a-z0-9-]*[a-z0-9]$/.test(normalized)) {
    setSubdomainStatus({
      state: 'error',
      message: 'Only lowercase letters, numbers, hyphens. No leading/trailing hyphen.'
    })
    return
  }

  setSubdomainStatus({ state: 'checking' })

  const res = await fetch(`/api/v1/auth/check-subdomain?subdomain=${normalized}`)
  const data = await res.json()

  setSubdomainStatus({
    state: data.available ? 'available' : 'taken',
    message: data.available ? `${data.previewUrl} is available` : data.reason
  })
}, 400)  // 400ms debounce
```

### Submit Gate

```typescript
// Only allow submit when:
// 1. Subdomain status is 'available'
// 2. All required fields are filled
// 3. No other validation errors

const canSubmit =
  subdomainStatus.state === 'available' &&
  firstName.trim().length > 0 &&
  lastName.trim().length > 0 &&
  email.trim().length > 0 &&
  password.length >= 8

<button type="submit" disabled={!canSubmit}>
  Create Store
</button>
```

### Register API Call

```typescript
const response = await fetch('/api/v1/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    firstName,
    lastName,
    email,
    password,
    subdomain: subdomain.toLowerCase().trim(),
    storeName: storeName.trim() || undefined,  // omit if empty
    deviceName: navigator.userAgent.slice(0, 50)
  })
})

if (response.status === 409) {
  // Race condition: someone grabbed the subdomain between check and submit
  // Re-run the availability check and show the error
  checkSubdomain(subdomain)
  return
}
```

### Visual States for the Subdomain Field

| State | UI |
|---|---|
| Empty / < 3 chars | No indicator |
| Checking | Spinner icon, "Checking availability..." |
| Available | ✅ Green, `https://myshop.kromic.in is available` |
| Taken | ❌ Red, reason message from API |
| Invalid format | ❌ Red, format error (client-side, no API call) |

---

## What Backend Creates on Registration

When registration succeeds, the backend automatically:
1. Creates a **Tenant** record (Status = Active)
2. Creates a **TenantDomain** record (subdomain = chosen value, isPrimary = true)
3. Creates the **User** with `TenantId` linked to the new tenant
4. Assigns **TenantAdmin** role to the user
5. Returns a JWT that already contains `tenantId` and `role: TenantAdmin`

The user is fully onboarded in a single API call — no second step needed.

---

## Reserved Subdomains (cannot be registered)

From `appsettings.json → MultiTenancy.ReservedSubdomains`:

```
store, storeapi, admin, api, auth, docs, health, status, cdn, assets
```

These are enforced both client-side (format check) and server-side (reserved service + DB check).
