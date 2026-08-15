# Email Verification - Frontend Implementation Guide

**Date:** July 31, 2026  
**Status:** Backend changes complete - Frontend changes required  
**Build Status:** ✅ 0 errors, 0 warnings

---

## 🔄 What Changed in Backend

### Login Behavior - Updated ✅

**Before:** Rejected login if email not verified (403 Forbidden)
```json
{
  "status": 403,
  "title": "Email Not Verified",
  "detail": "Email address has not been verified."
}
```

**Now:** Allows login but includes `IsEmailVerified` flag
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "...",
  "expiresInSeconds": 900,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "tenantId": null,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isEmailVerified": false,  // ← THIS FLAG - show banner if false
    "roles": ["Customer"]
  }
}
```

---

## 📋 Frontend Implementation Checklist

### 1. **Parse Login Response & Store Verification Status**

```typescript
// In your login API hook/service
const handleLogin = async (email: string, password: string) => {
  const response = await fetch('/api/v1/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password })
  });
  
  const data = await response.json();
  
  // Store both token AND verification status
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  localStorage.setItem('isEmailVerified', data.user.isEmailVerified);
  localStorage.setItem('userEmail', data.user.email);
  
  return data;
};
```

### 2. **Show Email Verification Banner**

**Location:** Display after login, above main content

**Conditions:**
- Show if: `isEmailVerified === false` AND user is logged in
- Hide if: User verified email (call verify endpoint)
- Hide if: User dismissed banner (store in state, but always show on page reload)

**UI Component Structure:**
```tsx
<VerificationBanner>
  ├─ Icon: warning/info icon
  ├─ Title: "Email Verification Required"
  ├─ Message: "Please verify your email to unlock all features"
  ├─ Buttons:
  │  ├─ "Send Verification Email" (resend endpoint)
  │  ├─ "I've Verified" (check status endpoint)
  │  └─ "Skip for now" (dismiss, but show on reload)
  └─ Styling: 
     └─ Yellow/amber background (warning style)
```

### 3. **Add Resend Verification Email Button**

**Endpoint:**
```
POST /api/v1/auth/resend-verification-email
```

**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Verification email sent to user@example.com"
}
```

**Frontend Implementation:**
```typescript
const handleResendVerification = async () => {
  setLoading(true);
  try {
    const response = await fetch('/api/v1/auth/resend-verification-email', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: userEmail })
    });
    
    if (response.ok) {
      setMessage("✓ Verification email sent. Check your inbox!");
      setShowBanner(true);
    } else {
      setError("Failed to send verification email");
    }
  } finally {
    setLoading(false);
  }
};
```

### 4. **Add Email Verification Check Button**

**Purpose:** Poll backend to see if user verified, update UI

**Endpoint:**
```
GET /api/v1/auth/me
```

**Response (200 OK):**
```json
{
  "id": "...",
  "email": "user@example.com",
  "isEmailVerified": true,  // ← Updated after user verifies
  "roles": ["Customer"]
}
```

**Frontend Implementation:**
```typescript
const handleCheckVerification = async () => {
  try {
    const response = await fetch('/api/v1/auth/me', {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });
    
    const userData = await response.json();
    
    if (userData.isEmailVerified) {
      // Update local state
      localStorage.setItem('isEmailVerified', 'true');
      setShowBanner(false);
      setMessage("✓ Email verified! All features unlocked.");
      
      // Refresh page or redirect after 2 seconds
      setTimeout(() => window.location.reload(), 2000);
    } else {
      setMessage("Email not verified yet. Check your inbox.");
    }
  } catch (error) {
    setError("Failed to check verification status");
  }
};
```

### 5. **Update Routes/Protected Features**

**Features to Keep Available (even without email verification):**
- ✅ View products/catalog
- ✅ View cart
- ✅ Search
- ✅ Public pages

**Features to Block (without email verification):**
- ❌ Add to cart → Show banner, allow to add but warn
- ❌ Checkout → Block with banner
- ❌ Create review → Show "Verify email first"
- ❌ Subscribe to newsletter → Show "Verify email first"

**Implementation Pattern:**
```typescript
const ProtectedAction = ({ isEmailVerified, onAction }) => {
  if (!isEmailVerified) {
    return (
      <Banner severity="warning">
        Please verify your email before {actionName}
        <Button onClick={() => showVerificationModal()}>
          Verify Now
        </Button>
      </Banner>
    );
  }
  
  return <Button onClick={onAction}>{actionName}</Button>;
};
```

---

## 🎯 UI Implementation Examples

### Example 1: Login Page → Post-Login State

**Before (old flow):**
```
Login Form → Submit → 403 Error "Email Not Verified" → Stuck
```

**After (new flow):**
```
Login Form 
  → Submit 
  → Redirect to Dashboard 
  → Show Yellow Banner: "Verify email to unlock all features"
  → User can still browse, but see warnings on sensitive actions
```

### Example 2: Verification Banner Component

```tsx
export function EmailVerificationBanner() {
  const [dismissed, setDismissed] = useState(false);
  const isEmailVerified = useSelector(state => state.auth.isEmailVerified);
  const isLoggedIn = useSelector(state => state.auth.isLoggedIn);
  const userEmail = useSelector(state => state.auth.email);
  
  if (dismissed || !isLoggedIn || isEmailVerified) return null;
  
  return (
    <Alert 
      severity="warning" 
      sx={{ mb: 2 }}
      action={
        <>
          <Button size="small" onClick={handleResendEmail}>
            Resend Email
          </Button>
          <Button size="small" onClick={handleCheckVerification}>
            Already Verified?
          </Button>
          <IconButton size="small" onClick={() => setDismissed(true)}>
            <CloseIcon />
          </IconButton>
        </>
      }
    >
      <AlertTitle>Email Verification Required</AlertTitle>
      Please verify your email ({userEmail}) to unlock all features. 
      A verification link was sent to your email.
    </Alert>
  );
}
```

### Example 3: Checkout Page - Block Without Verification

```tsx
export function CheckoutPage() {
  const isEmailVerified = useSelector(state => state.auth.isEmailVerified);
  const isLoggedIn = useSelector(state => state.auth.isLoggedIn);
  
  if (!isLoggedIn) {
    return <Redirect to="/login" />;
  }
  
  if (!isEmailVerified) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <WarningIcon sx={{ fontSize: 64, color: 'warning.main', mb: 2 }} />
        <Typography variant="h5" gutterBottom>
          Email Verification Required
        </Typography>
        <Typography color="textSecondary" paragraph>
          Please verify your email before checking out.
        </Typography>
        <Stack spacing={2} direction="row" justifyContent="center">
          <Button 
            variant="contained" 
            onClick={handleResendVerification}
          >
            Send Verification Email
          </Button>
          <Button 
            variant="outlined"
            onClick={() => navigate('/dashboard')}
          >
            Go Back
          </Button>
        </Stack>
      </Box>
    );
  }
  
  return <CheckoutForm />;
}
```

### Example 4: Add-to-Cart Warning

```tsx
export function ProductCard({ product }) {
  const isEmailVerified = useSelector(state => state.auth.isEmailVerified);
  const isLoggedIn = useSelector(state => state.auth.isLoggedIn);
  
  const handleAddToCart = async () => {
    if (!isLoggedIn) {
      navigate('/login');
      return;
    }
    
    if (!isEmailVerified) {
      // Show warning but still add to cart
      showSnackbar({
        message: "Verify your email before checking out",
        action: <Button onClick={showVerificationBanner}>Verify Now</Button>,
        severity: 'warning'
      });
    }
    
    // Add to cart regardless
    await addToCart(product.id);
  };
  
  return (
    <Button 
      variant="contained" 
      onClick={handleAddToCart}
      fullWidth
    >
      Add to Cart
    </Button>
  );
}
```

---

## 📱 Where to Show the Banner

### Primary Location (Most Important):
- **Dashboard/Home page** - Top of page after login
- **Navbar** - Small banner icon for quick action
- **Modals** - When user tries to checkout without verification

### Secondary Locations:
- **Settings page** - Account section showing verification status
- **Help/Support** - FAQ about email verification
- **Profile page** - Show verification status

---

## 🔄 User Experience Flow

### Scenario 1: New User Signs Up

```
1. User registers (email not yet verified)
   ↓
2. Gets JWT token in response
   ↓
3. Redirected to dashboard
   ↓
4. Yellow banner shows: "Verify your email - Check your inbox"
   ↓
5. User clicks link in email
   ↓
6. Verification API called (no change needed in FE)
   ↓
7. FE detects isEmailVerified = true on next page load/API call
   ↓
8. Banner disappears automatically
```

### Scenario 2: User Forgot to Verify

```
1. User logs in (isEmailVerified = false in response)
   ↓
2. Dashboard shows banner
   ↓
3. User clicks "Resend Verification Email"
   ↓
4. Email sent successfully
   ↓
5. Snackbar: "Check your email - link expires in 24 hours"
   ↓
6. User clicks link in email
   ↓
7. FE checks verification status next page reload
   ↓
8. Banner disappears
```

### Scenario 3: User Tries to Checkout Without Verification

```
1. User clicks "Checkout"
   ↓
2. Checkout page detects isEmailVerified = false
   ↓
3. Shows blocking page: "Please verify email first"
   ↓
4. User clicks "Resend Verification Email" button
   ↓
5. Sends email, shows success message
   ↓
6. User verifies in email client
   ↓
7. FE detects verified status
   ↓
8. "Go to Checkout" button appears / auto-redirects
```

---

## 🛠️ Implementation Checklist

### Phase 1: Core Changes (ESSENTIAL)
- [ ] Parse `isEmailVerified` from login response
- [ ] Store in localStorage/Redux
- [ ] Create VerificationBanner component
- [ ] Show banner on dashboard if not verified
- [ ] Style warning banner (yellow/amber)

### Phase 2: User Actions (REQUIRED)
- [ ] Add "Resend Verification Email" button
- [ ] Add "Check Verification Status" button
- [ ] Wire to backend endpoints
- [ ] Show success/error messages

### Phase 3: Feature Gating (RECOMMENDED)
- [ ] Block checkout if not verified
- [ ] Warn on add-to-cart if not verified
- [ ] Warn on review submission if not verified
- [ ] Warn on newsletter signup if not verified

### Phase 4: Polish (NICE TO HAVE)
- [ ] Auto-refresh verification status every 30 seconds (while banner shows)
- [ ] Confetti animation when verified ✨
- [ ] Email verification confirmation page
- [ ] Resend email rate limiting (show "Already sent, try again in 60 seconds")

---

## 📊 Response Examples

### Login with Unverified Email
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "GhbhjSHkshdjhsk==",
  "expiresInSeconds": 900,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "tenantId": null,
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isEmailVerified": false,
    "roles": ["Customer"]
  }
}
```

### After Email Verification (GET /auth/me)
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "tenantId": null,
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isEmailVerified": true,
  "roles": ["Customer"]
}
```

### Resend Verification Email
```json
{
  "success": true,
  "message": "Verification email sent to john@example.com"
}
```

---

## ✅ Testing Checklist

1. **Login Flow:**
   - [ ] Unverified user can login successfully
   - [ ] Token received and stored
   - [ ] `isEmailVerified = false` in response
   - [ ] Redirects to dashboard (not blocked)

2. **Banner Display:**
   - [ ] Banner shows on dashboard
   - [ ] Shows user's email address
   - [ ] Close button dismisses banner (but shows on reload)
   - [ ] Styling is consistent with app theme

3. **Resend Email:**
   - [ ] Button enabled and clickable
   - [ ] API call succeeds
   - [ ] User sees success message
   - [ ] Email received in inbox

4. **Check Verification:**
   - [ ] Button calls /api/v1/auth/me
   - [ ] Shows appropriate message (verified/not verified)
   - [ ] Banner disappears if verified
   - [ ] Page reloads automatically

5. **Protected Features:**
   - [ ] Checkout shows blocking screen if not verified
   - [ ] Can still add to cart (with warning)
   - [ ] Can still browse products
   - [ ] Reviews show warning if not verified

---

## 🚀 Deployment Notes

**Backend Changes:** ✅ Already deployed and built
- Login no longer throws 403 exception
- `isEmailVerified` flag always included in auth response
- Verification endpoints already exist

**Frontend Changes:** Required (listed above)
- No backend API changes needed
- No new endpoints to integrate
- Just use existing endpoints with new logic

---

## 📞 Questions?

If you need to:
- **Block more features** - Use the `isEmailVerified` flag check in your FE components
- **Change the message** - Update banner text (backend message already set)
- **Add verification deadline** - Add `verificationTokenExpiresAt` to User entity
- **Add verification attempts limit** - Already handled by backend

