# Kromic Store Backend Documentation

# Phase 06 -- 96 JWT & Refresh Tokens

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the token-based authentication strategy for Kromic
Store using JSON Web Tokens (JWT) and Refresh Tokens. The architecture
provides secure, stateless API authentication while supporting
multi-device sessions, token rotation, revocation, and tenant-aware
identity.

------------------------------------------------------------------------

# Objectives

-   Enable stateless authentication
-   Secure API access
-   Support long-lived user sessions
-   Prevent token replay attacks
-   Enable token revocation
-   Support multi-tenant identities

------------------------------------------------------------------------

# Token Types

## Access Token

Used for:

-   API authentication
-   Authorization
-   User identity

Characteristics:

-   Short-lived
-   Self-contained
-   Digitally signed
-   Never persisted by the server

Recommended lifetime:

-   15--30 minutes

------------------------------------------------------------------------

## Refresh Token

Used to obtain a new access token.

Characteristics:

-   Long-lived
-   Random, cryptographically secure
-   Stored server-side (hashed)
-   Bound to a session

Recommended lifetime:

-   30--90 days

------------------------------------------------------------------------

# JWT Structure

A JWT consists of:

1.  Header
2.  Payload
3.  Signature

Only signed tokens should be accepted.

------------------------------------------------------------------------

# Recommended Claims

Required:

-   sub (UserId)
-   tenant_id
-   session_id
-   jti
-   iat
-   exp

Optional:

-   roles
-   permissions (or permission version)
-   store_id
-   locale

Avoid placing sensitive information inside the payload.

------------------------------------------------------------------------

# Signing

Recommendations:

-   HMAC-SHA256 (HS256) for symmetric deployments
-   RS256/ES256 for asymmetric key management
-   Rotate signing keys regularly
-   Store keys securely using environment secrets or a secret manager

------------------------------------------------------------------------

# Refresh Token Rotation

Each successful refresh should:

1.  Validate refresh token
2.  Issue new access token
3.  Issue new refresh token
4.  Revoke previous refresh token
5.  Persist new session state

This limits replay attacks using stolen refresh tokens.

------------------------------------------------------------------------

# Session Management

Track sessions with:

-   SessionId
-   UserId
-   TenantId
-   Device information
-   IP address (optional)
-   User agent
-   CreatedAt
-   LastUsedAt
-   ExpiresAt
-   RevokedAt

Support multiple active sessions per user.

------------------------------------------------------------------------

# Revocation

Revoke tokens when:

-   User logs out
-   Password changes
-   Account disabled
-   Suspicious activity detected
-   Administrator forces sign-out

Revocation should invalidate the associated refresh token immediately.

------------------------------------------------------------------------

# Expiration

Access Tokens:

-   Expire automatically

Refresh Tokens:

-   Sliding or fixed expiration
-   Expired tokens cannot be renewed

Require re-authentication after refresh expiration.

------------------------------------------------------------------------

# Replay Protection

Implement:

-   One-time refresh token usage
-   Unique JWT ID (jti)
-   Session binding
-   Refresh token rotation
-   Reuse detection

Treat reuse of a revoked refresh token as a potential compromise.

------------------------------------------------------------------------

# Multi-Device Support

Allow independent sessions for:

-   Web browsers
-   Mobile devices
-   Tablets

Users should be able to:

-   View active sessions
-   Revoke specific sessions
-   Revoke all sessions except current

------------------------------------------------------------------------

# Storage

Client:

-   Secure, HTTP-only cookies for browser-based apps where applicable
-   Secure platform storage for native apps

Server:

-   Store only hashed refresh tokens
-   Never store plaintext tokens

------------------------------------------------------------------------

# Error Handling

Return standardized responses for:

-   Expired access token
-   Invalid signature
-   Revoked refresh token
-   Expired refresh token
-   Session not found
-   Tenant mismatch

Do not leak implementation details.

------------------------------------------------------------------------

# Security

-   HTTPS only
-   Secure cookies where applicable
-   SameSite protection
-   Short-lived access tokens
-   Hash refresh tokens
-   Audit token events
-   Rotate signing keys

------------------------------------------------------------------------

# Observability

Record:

-   Token issuance
-   Token refresh
-   Token revocation
-   Refresh reuse detection
-   Session termination

Include correlation identifiers for tracing.

------------------------------------------------------------------------

# Testing

Verify:

-   Token validation
-   Expiration
-   Refresh rotation
-   Replay protection
-   Multi-device sessions
-   Revocation behavior
-   Tenant-aware claims

------------------------------------------------------------------------

# Best Practices

-   Keep access tokens short-lived.
-   Rotate refresh tokens after every use.
-   Hash refresh tokens before storage.
-   Design for multiple concurrent sessions.
-   Audit all token lifecycle events.

------------------------------------------------------------------------

# Next Document

**97 -- Identity Management**

Topics:

-   User lifecycle
-   Identity model
-   Roles
-   Invitations
-   Account states
-   Profile management
-   Administrative operations
