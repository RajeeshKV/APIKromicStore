# Kromic Store Backend Implementation Guide

# Phase 03 -- 24 Authentication and Authorization APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define authentication, authorization, identity lifecycle, and security
endpoints for Kromic Store.

------------------------------------------------------------------------

# Authentication Flow

``` text
Register/Login
      ↓
Validate Credentials
      ↓
Issue JWT + Refresh Token
      ↓
Access Protected APIs
      ↓
Refresh Access Token
      ↓
Logout / Token Revocation
```

------------------------------------------------------------------------

# Endpoint Catalog

  Method   Endpoint                       Auth   Description
  -------- ------------------------------ ------ -------------------------------
  POST     /api/v1/auth/register          No     Register tenant user/customer
  POST     /api/v1/auth/login             No     Email & password login
  POST     /api/v1/auth/google            No     Google Sign-In
  POST     /api/v1/auth/refresh           No     Refresh access token
  POST     /api/v1/auth/logout            Yes    Revoke refresh token
  POST     /api/v1/auth/forgot-password   No     Request reset email
  POST     /api/v1/auth/reset-password    No     Complete password reset
  GET      /api/v1/auth/verify-email      No     Verify email
  GET      /api/v1/auth/me                Yes    Current user profile

------------------------------------------------------------------------

# Login Request

``` json
{
  "email":"user@example.com",
  "password":"********"
}
```

Response:

``` json
{
  "accessToken":"...",
  "refreshToken":"...",
  "expiresIn":3600
}
```

------------------------------------------------------------------------

# Registration Rules

Required:

-   First name
-   Last name
-   Email
-   Password

Validation:

-   Unique email within tenant
-   Valid password policy
-   Verified email required before privileged actions

------------------------------------------------------------------------

# Password Policy

-   Minimum 8 characters
-   Uppercase
-   Lowercase
-   Number
-   Special character
-   Prevent common passwords

Passwords are hashed using ASP.NET Core PasswordHasher.

------------------------------------------------------------------------

# Google Sign-In

Flow:

1.  Client obtains Google ID token
2.  API validates token
3.  User created if first login
4.  JWT issued
5.  Refresh token stored

------------------------------------------------------------------------

# Refresh Token

Rules:

-   Rotate on every refresh
-   Store hashed value
-   Revoke previous token
-   Expiration configurable
-   Detect replay attempts

------------------------------------------------------------------------

# Logout

-   Revoke active refresh token
-   Access token expires naturally
-   Support logout from all devices (future)

------------------------------------------------------------------------

# Email Verification

Flow:

1.  Generate verification token
2.  Send email
3.  Validate token
4.  Mark email verified
5.  Invalidate token

------------------------------------------------------------------------

# Password Reset

Flow:

1.  Generate reset token
2.  Send email
3.  Validate token
4.  Update password
5.  Revoke all refresh tokens

------------------------------------------------------------------------

# Authorization Roles

-   SuperUser
-   TenantAdmin
-   StoreManager
-   Customer

Authorization uses policies and claims, not hardcoded role checks.

------------------------------------------------------------------------

# JWT Claims

Required claims:

-   sub (UserId)
-   tenantId
-   email
-   role
-   jti

------------------------------------------------------------------------

# Security

-   HTTPS only
-   Secure, HttpOnly refresh token cookie (web) or secure storage
    (mobile)
-   CSRF protection where applicable
-   Rate limit login endpoints
-   Account lockout after repeated failures
-   Audit login events

------------------------------------------------------------------------

# Error Scenarios

  Scenario                  Status
  ----------------------- --------
  Invalid credentials          401
  Email not verified           403
  Expired refresh token        401
  Invalid reset token          400
  Account locked               423

------------------------------------------------------------------------

# Testing

Verify:

-   Registration
-   Login
-   Google Sign-In
-   Refresh rotation
-   Logout
-   Email verification
-   Password reset
-   Authorization policies

------------------------------------------------------------------------

# Next Document

**25-Tenant-Management-APIs.md**

Topics:

-   Tenant onboarding
-   Store configuration
-   Branding
-   Domains
-   Settings
-   Subscription management
