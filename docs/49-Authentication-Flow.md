# Kromic Store Frontend Documentation

# Phase 04 -- 49 Authentication Flow

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the complete authentication and session management experience for
the Kromic Store frontend.

The frontend must provide a secure, seamless authentication flow for
Super Admins, Tenant Admins, Store Managers, Staff, and Customers.

------------------------------------------------------------------------

# Goals

-   Secure authentication
-   Seamless session management
-   Minimal login friction
-   Consistent authorization
-   Automatic token refresh
-   Multi-tab synchronization

------------------------------------------------------------------------

# Supported Authentication

-   Email & Password
-   Google Sign-In
-   Password Reset
-   Email Verification

Future:

-   Microsoft Login
-   Passkeys
-   Multi-Factor Authentication (MFA)

------------------------------------------------------------------------

# Authentication Flow

``` text
User
    ↓
Login Screen
    ↓
API Authentication
    ↓
Access Token
    ↓
Refresh Token
    ↓
Load Current User
    ↓
Role & Permissions
    ↓
Redirect to Dashboard/Storefront
```

------------------------------------------------------------------------

# Login Flow

1.  Validate form.
2.  Submit credentials.
3.  Receive access and refresh tokens.
4.  Retrieve current user profile.
5.  Load tenant configuration.
6.  Redirect based on role.

------------------------------------------------------------------------

# Registration Flow

Steps:

-   Account creation
-   Email verification
-   Profile completion
-   Initial login

Tenant registration should redirect into the store setup wizard.

------------------------------------------------------------------------

# Forgot Password

Flow:

1.  Enter email.
2.  Receive reset email.
3.  Open secure reset link.
4.  Create new password.
5.  Redirect to login.

------------------------------------------------------------------------

# Email Verification

After registration:

-   Display pending verification screen.
-   Allow resend verification email.
-   Prevent protected actions until verified.

------------------------------------------------------------------------

# Session Management

Frontend responsibilities:

-   Detect expired access tokens.
-   Refresh tokens automatically.
-   Retry the failed request once.
-   Redirect to login if refresh fails.

------------------------------------------------------------------------

# Token Storage

Recommended:

-   Access Token: In-memory
-   Refresh Token: Secure HttpOnly cookie (preferred)

Avoid storing sensitive tokens in localStorage.

------------------------------------------------------------------------

# Protected Navigation

Authenticated users only:

-   Super Admin Portal
-   Tenant Admin Portal
-   Account pages
-   Checkout history

Guests may access storefront browsing.

------------------------------------------------------------------------

# Authorization

Render UI based on:

-   Role
-   Permissions
-   Tenant status
-   Subscription features
-   Feature flags

Never rely solely on frontend authorization.

------------------------------------------------------------------------

# Logout

Logout should:

-   Revoke refresh token
-   Clear client state
-   Clear cached queries
-   Redirect to login
-   Synchronize across browser tabs

------------------------------------------------------------------------

# Session Expiration

If the session expires:

-   Attempt silent refresh.
-   If unsuccessful:
    -   Preserve intended destination.
    -   Redirect to login.
    -   Continue after successful authentication.

------------------------------------------------------------------------

# Multi-Tab Synchronization

Keep browser tabs synchronized for:

-   Login
-   Logout
-   Session expiration
-   Profile updates

------------------------------------------------------------------------

# Error Handling

Handle gracefully:

-   Invalid credentials
-   Locked account
-   Email not verified
-   Expired reset links
-   Network failures
-   Server errors

Provide clear, user-friendly messages.

------------------------------------------------------------------------

# Security Best Practices

-   Never expose refresh tokens to JavaScript.
-   Protect against XSS.
-   Use HTTPS exclusively.
-   Validate redirects.
-   Clear sensitive state on logout.

------------------------------------------------------------------------

# Testing

Verify:

-   Login
-   Registration
-   Password reset
-   Email verification
-   Automatic refresh
-   Logout
-   Protected routes
-   Multi-tab synchronization

------------------------------------------------------------------------

# Next Document

**50-Layout-Architecture.md**

Topics:

-   Shared layouts
-   Super Admin layout
-   Tenant Admin layout
-   Storefront layout
-   Navigation
-   Responsive behavior
-   Layout composition
