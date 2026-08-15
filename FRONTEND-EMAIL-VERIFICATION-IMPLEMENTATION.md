# Frontend Email Verification - Implementation Guide

**Status:** Production Implementation Plan  
**Date:** 2026-07-31

---

## Overview

When user clicks email verification link, they are redirected to the frontend with a token. The frontend must handle this token and refresh the JWT, showing the banner removal after verification succeeds.

---

## Architecture Flow

```
1. User receives email with link:
   https://admin.kromic.in/verify-email?token=ABC123&email=user@example.com

2. User clicks link → Frontend page loads with token + email

3. Frontend extracts token from URL, calls backend:
   POST /api/v1/auth/verify-email?token=ABC123

4. Backend verifies email, marks user as verified

5. Frontend calls refresh endpoint:
   POST /api/v1/auth/refresh-token

6. Gets NEW JWT with isEmailVerified: true

7. Stores new JWT, banner disappears automatically
```

---

## Frontend Implementation Details

### Step 1: Create Email Verification Page

**Location:** `web/web-admin/src/pages/VerifyEmailPage.tsx` (NEW FILE)

```typescript
import React, { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { authService } from '@kromic/shared-api'

export function VerifyEmailPage(): React.ReactNode {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const qc = useQueryClient()
  
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading')
  const [message, setMessage] = useState('')

  useEffect(() => {
    const handleVerification = async () => {
      const token = searchParams.get('token')
      const email = searchParams.get('email')

      if (!token || !email) {
        setStatus('error')
        setMessage('Invalid verification link')
        return
      }

      try {
        // Step 1: Verify email on backend
        await authService.verifyEmail(token, email)
        
        // Step 2: Refresh JWT to get updated isEmailVerified claim
        const refreshToken = localStorage.getItem('refresh_token')
        if (!refreshToken) {
          setStatus('error')
          setMessage('Session expired, please login again')
          return
        }

        const response = await authService.refresh({ refreshToken })
        
        // Step 3: Save new JWT
        if (response.accessToken && response.refreshToken) {
          authService.saveTokens(response.accessToken, response.refreshToken)
        }

        // Step 4: Invalidate current user query to refresh in all components
        qc.invalidateQueries({ queryKey: ['auth', 'me'] })

        // Step 5: Show success and redirect
        setStatus('success')
        setMessage('Email verified successfully!')
        
        setTimeout(() => {
          navigate('/', { replace: true })
        }, 2000)
      } catch (error) {
        setStatus('error')
        setMessage(error instanceof Error ? error.message : 'Verification failed')
      }
    }

    handleVerification()
  }, [searchParams, navigate, qc])

  if (status === 'loading') {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="w-12 h-12 border-4 border-primary/20 border-t-primary rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-on-surface-variant">Verifying your email...</p>
        </div>
      </div>
    )
  }

  if (status === 'success') {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="w-12 h-12 bg-success/20 rounded-full flex items-center justify-center mx-auto mb-4">
            <span className="material-symbols-outlined text-success">check_circle</span>
          </div>
          <h1 className="font-headline-lg text-on-surface mb-2">Email Verified!</h1>
          <p className="text-on-surface-variant mb-4">Redirecting to dashboard...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="text-center">
        <div className="w-12 h-12 bg-error/20 rounded-full flex items-center justify-center mx-auto mb-4">
          <span className="material-symbols-outlined text-error">error</span>
        </div>
        <h1 className="font-headline-lg text-on-surface mb-2">Verification Failed</h1>
        <p className="text-on-surface-variant mb-4">{message}</p>
        <button
          onClick={() => navigate('/login', { replace: true })}
          className="px-6 py-2 bg-primary text-on-primary rounded-lg hover:bg-primary/90"
        >
          Back to Login
        </button>
      </div>
    </div>
  )
}
```

---

### Step 2: Update authService

**Location:** `packages/shared-api/src/services/authService.ts` (ADD METHOD)

```typescript
export const authService = {
  // ... existing methods ...

  /**
   * Verify email using token from email link.
   * Backend marks user's email as verified.
   */
  async verifyEmail(token: string, email: string): Promise<void> {
    return httpClient({ 
      method: 'POST', 
      url: '/api/v1/auth/verify-email',
      data: { token, email }
    })
  },

  // ... rest of methods ...
}
```

---

### Step 3: Update Router

**Location:** `web/web-admin/src/routes/AdminRouter.tsx` (ADD ROUTE)

```typescript
import { VerifyEmailPage } from '../pages/VerifyEmailPage'

// In your routes array, add:
{
  path: '/verify-email',
  element: <VerifyEmailPage />,
  // This route should NOT require authentication
  // because the user hasn't verified email yet
}
```

---

### Step 4: Update Auth Layout to Show Banner

**Location:** `web/web-admin/src/layouts/AuthLayout.tsx` or main layout (EXISTING UPDATE)

```typescript
import { useCurrentUser } from '@kromic/shared-api'
import jwt_decode from 'jwt-decode'

export function AuthLayout({ children }: { children: React.ReactNode }) {
  const { data: user } = useCurrentUser()
  
  // Option 1: Read from JWT directly (immediate)
  const token = localStorage.getItem('auth_token')
  let isEmailVerified = false
  if (token) {
    try {
      const decoded = jwt_decode(token) as any
      isEmailVerified = decoded.isEmailVerified
    } catch (e) {
      // JWT decode error, fall back to user data
      isEmailVerified = user?.isEmailVerified ?? false
    }
  }

  // Option 2: Or use user data from API (if you want to be extra safe)
  // isEmailVerified = user?.isEmailVerified ?? false

  return (
    <div>
      {/* Email Verification Banner */}
      {!isEmailVerified && user && (
        <EmailVerificationBanner userEmail={user.email} />
      )}

      {/* Main content */}
      {children}
    </div>
  )
}
```

---

### Step 5: Create Email Verification Banner Component

**Location:** `web/web-admin/src/components/EmailVerificationBanner.tsx` (NEW FILE)

```typescript
import React, { useState } from 'react'
import { useResendVerification } from '@kromic/shared-api'

interface EmailVerificationBannerProps {
  userEmail?: string
}

export function EmailVerificationBanner({ 
  userEmail 
}: EmailVerificationBannerProps): React.ReactNode {
  const [dismissed, setDismissed] = useState(false)
  const resendVerification = useResendVerification()

  if (dismissed) return null

  return (
    <div className="bg-warning-container border-b border-warning/30 px-4 py-3 flex items-center justify-between">
      <div className="flex items-center gap-3">
        <span className="material-symbols-outlined text-warning">info</span>
        <div>
          <p className="font-body-md text-on-surface">
            Please verify your email address
          </p>
          <p className="font-body-sm text-on-surface-variant">
            Check {userEmail} for a verification link
          </p>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <button
          onClick={() => resendVerification.mutate(userEmail!)}
          disabled={resendVerification.isPending}
          className="px-3 py-1 text-sm font-body-sm text-warning hover:bg-warning/10 rounded"
        >
          {resendVerification.isPending ? 'Sending...' : 'Resend'}
        </button>
        <button
          onClick={() => setDismissed(true)}
          className="text-on-surface-variant hover:text-on-surface"
        >
          <span className="material-symbols-outlined">close</span>
        </button>
      </div>
    </div>
  )
}
```

---

### Step 6: Update useCurrentUser Hook (if needed)

**Location:** `packages/shared-api/src/hooks/index.ts` (UPDATE)

```typescript
export function useCurrentUser() {
  return useQuery({
    queryKey: queryKeys.currentUser(),
    queryFn: () => authService.me(),
    enabled: authService.isAuthenticated(),
    staleTime: 5 * 60 * 1000,
    refetchOnWindowFocus: true,  // ← Add this so banner updates immediately
  })
}
```

---

## Backend Changes Required (Already Done ✓)

### Backend Email Verification Endpoint

**Ensure backend has:** `POST /api/v1/auth/verify-email`

**Should accept:**
```json
{
  "token": "email-verification-token",
  "email": "user@example.com"
}
```

**Should do:**
1. Validate token matches email
2. Mark user's `IsEmailVerified = true`
3. Return 200 OK

---

## Flow Summary

### User Journey

```
1. User registers
   ↓
2. Email sent with link:
   https://admin.kromic.in/verify-email?token=XYZ&email=user@email.com
   ↓
3. User clicks link
   ↓
4. VerifyEmailPage loads
   ↓
5. Frontend extracts token, calls POST /api/v1/auth/verify-email
   ↓
6. Backend marks email as verified
   ↓
7. Frontend calls POST /api/v1/auth/refresh-token
   ↓
8. Backend returns NEW JWT with isEmailVerified: true
   ↓
9. Frontend saves JWT, invalidates queries
   ↓
10. AuthLayout detects isEmailVerified=true, hides banner
    ↓
11. User redirected to dashboard with no banner ✅
```

---

## File Changes Summary

| Location | Type | Purpose |
|----------|------|---------|
| `web/web-admin/src/pages/VerifyEmailPage.tsx` | NEW | Handle email verification callback |
| `web/web-admin/src/routes/AdminRouter.tsx` | UPDATE | Add /verify-email route |
| `web/web-admin/src/layouts/AuthLayout.tsx` | UPDATE | Show/hide banner based on isEmailVerified |
| `web/web-admin/src/components/EmailVerificationBanner.tsx` | NEW | Banner component with resend logic |
| `packages/shared-api/src/services/authService.ts` | UPDATE | Add verifyEmail() method |
| `packages/shared-api/src/hooks/index.ts` | UPDATE | Ensure refetchOnWindowFocus=true |

---

## Best Production Practices

### 1. Token Security
- ✅ Token should be time-limited (e.g., 24 hours)
- ✅ Token should be one-time use only
- ✅ Token should be hashed in database

### 2. UI/UX
- ✅ Clear loading state while verifying
- ✅ Success/error messages
- ✅ Auto-redirect after verification
- ✅ Resend button in banner

### 3. Error Handling
- ✅ Invalid/expired token → show error
- ✅ Network error → retry button
- ✅ User not authenticated → redirect to login

### 4. JWT Token Refresh
- ✅ After verification, call refresh endpoint
- ✅ Store new JWT with isEmailVerified: true
- ✅ Invalidate queries so all components update

### 5. Banner Logic
- ✅ Read isEmailVerified from JWT (immediate)
- ✅ Fallback to API response (safe)
- ✅ Dismiss button available (UX friendly)
- ✅ Resend verification email button

---

## Testing Checklist

- [ ] Create new user without verifying email
- [ ] See verification banner on dashboard
- [ ] Click resend verification email
- [ ] Receive email with link
- [ ] Click link, redirected to /verify-email?token=...&email=...
- [ ] See loading state while verifying
- [ ] Backend marks email as verified
- [ ] Frontend calls refresh, gets new JWT
- [ ] Banner automatically disappears
- [ ] Redirected to dashboard
- [ ] JWT now contains isEmailVerified: true
- [ ] Refresh page, banner still gone (JWT persisted)

---

## Notes

- **Token in URL:** Use POST body or secure header, not URL for production (HTTPS helps)
- **Auto-redirect timing:** 2 seconds allows user to see success message
- **Banner persistence:** Stays until next page refresh (because JWT is cached)
- **Resend flow:** Uses existing useResendVerification hook
- **No auth required for /verify-email route** since user hasn't verified yet

---

**Status:** Ready for Implementation  
**Complexity:** Medium  
**Time Estimate:** 2-3 hours
