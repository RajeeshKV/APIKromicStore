# Kromic Store Backend Implementation Guide

# Phase 02 -- 11 Authentication Database

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the authentication and authorization database model for all user
types.

Supported identities:

-   Super User
-   Tenant User
-   Customer

------------------------------------------------------------------------

# Authentication Strategy

-   JWT Access Token
-   Refresh Token
-   Token Versioning
-   Google OAuth
-   Email & Password
-   Email Verification
-   Password Reset

------------------------------------------------------------------------

# User Types

## Super User

Responsibilities:

-   Platform administration
-   Tenant management
-   Subscription management
-   Theme publishing

Only a few records should exist.

------------------------------------------------------------------------

## Tenant User

Belongs to exactly one tenant.

Capabilities:

-   Manage store
-   Orders
-   Products
-   Themes
-   Customers
-   Reports

Multiple tenant users are supported.

------------------------------------------------------------------------

## Customer

Customer belongs to exactly one tenant.

Same email address may exist across different tenants.

------------------------------------------------------------------------

# Tables

## Users

Purpose:

Stores authentication credentials.

Columns:

-   Id (Guid)
-   TenantId (nullable for Super User)
-   Email
-   PasswordHash
-   FirstName
-   LastName
-   PhoneNumber
-   IsEmailVerified
-   IsActive
-   TokenVersion
-   LastLoginOnUtc

Audit and soft delete fields apply.

Unique Indexes:

-   UX_Users_Email_Tenant

------------------------------------------------------------------------

## Roles

Examples:

-   SuperAdmin
-   TenantOwner
-   StoreManager
-   OrderManager
-   Customer

------------------------------------------------------------------------

## UserRoles

Many-to-many relationship between Users and Roles.

Indexes:

-   UserId
-   RoleId

------------------------------------------------------------------------

## RefreshTokens

Purpose:

Manage long-lived sessions.

Columns:

-   Id
-   UserId
-   TokenHash
-   ExpiresOnUtc
-   RevokedOnUtc
-   CreatedOnUtc
-   DeviceName
-   IPAddress

Rules:

-   Store hashed tokens only.
-   Revoke on logout.
-   Revoke all when TokenVersion changes.

------------------------------------------------------------------------

## EmailVerificationTokens

Columns:

-   Id
-   UserId
-   TokenHash
-   ExpiresOnUtc
-   ConsumedOnUtc

Expire automatically.

------------------------------------------------------------------------

## PasswordResetTokens

Columns:

-   Id
-   UserId
-   TokenHash
-   ExpiresOnUtc
-   ConsumedOnUtc

One-time use only.

------------------------------------------------------------------------

# Google Authentication

Store:

-   GoogleSubjectId
-   AuthenticationProvider

Users can authenticate using:

-   Local account
-   Google

------------------------------------------------------------------------

# Authorization

Policy-based authorization.

Permissions are enforced in the application layer.

Controllers should remain thin.

------------------------------------------------------------------------

# Security Rules

-   Passwords stored using ASP.NET Core PasswordHasher.
-   Never store plaintext passwords.
-   Refresh tokens are hashed.
-   Email verification required.
-   Security-sensitive actions require authenticated users.

------------------------------------------------------------------------

# Recommended Indexes

-   UX_Users_Email_Tenant
-   IX_RefreshTokens_UserId
-   IX_EmailVerification_UserId
-   IX_PasswordReset_UserId

------------------------------------------------------------------------

# Relationships

``` text
Users
  │
  ├── RefreshTokens
  ├── PasswordResetTokens
  ├── EmailVerificationTokens
  └── UserRoles
          │
          └── Roles
```

------------------------------------------------------------------------

# Business Rules

-   Tenant users cannot access another tenant.
-   Customer accounts are tenant scoped.
-   Super User bypasses tenant restrictions for platform administration
    only.
-   Disable users instead of deleting them.

------------------------------------------------------------------------

# Testing

Verify:

-   Login
-   Refresh token rotation
-   Email verification
-   Password reset
-   Role authorization
-   Cross-tenant isolation

------------------------------------------------------------------------

# Next Document

**12-Tenant-And-Store-Database.md**

Topics:

-   Tenant entity
-   Store configuration
-   Contact information
-   Branding
-   Domains
-   Social links
-   Business settings
-   Subscription metadata
