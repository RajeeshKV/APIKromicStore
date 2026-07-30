# Kromic Store Backend Implementation Guide

# Phase 01 - 06 Environment Variables

**Version:** 1.0\
**Status:** Foundation

------------------------------------------------------------------------

# Purpose

This document defines every environment variable used by Kromic Store.

Goals:

-   Zero secrets in source control
-   Easy Render deployment
-   Strong startup validation
-   Environment-driven configuration

------------------------------------------------------------------------

# Configuration Loading Order

1.  appsettings.json
2.  appsettings.{Environment}.json
3.  User Secrets (Development)
4.  Environment Variables
5.  Command-line arguments

Environment variables always take precedence.

------------------------------------------------------------------------

# Naming Convention

Use nested configuration keys.

Examples:

-   Application\_\_Name
-   Jwt\_\_Issuer
-   Database\_\_ConnectionString
-   Cloudinary\_\_CloudName

------------------------------------------------------------------------

# Application

  ------------------------------------------------------------------------------------------------
  Variable                          Required         Description           Example
  --------------------------------- ---------------- --------------------- -----------------------
  ASPNETCORE_ENVIRONMENT            Yes              Runtime environment   Production

  Application\_\_Name               Yes              Application name      Kromic Store API

  Application\_\_BaseUrl            Yes              Public API URL        https://api.kromic.in

  Application\_\_FrontendUrl        Yes              Landing page URL      https://kromic.in

  Application\_\_StorefrontDomain   Yes              Base storefront       kromic.in
                                                     domain                
  ------------------------------------------------------------------------------------------------

------------------------------------------------------------------------

# Database

  -------------------------------------------------------------------------------------
  Variable                             Required             Description
  ------------------------------------ -------------------- ---------------------------
  Database\_\_ConnectionString         Yes                  PostgreSQL connection
                                                            string

  Database\_\_CommandTimeout           No                   EF timeout (Default 30)

  Database\_\_EnableSensitiveLogging   No                   Enable EF sensitive logging
                                                            (Development only)
  -------------------------------------------------------------------------------------

------------------------------------------------------------------------

# JWT

  Variable                    Required   Description
  --------------------------- ---------- ------------------------------
  Jwt\_\_Issuer               Yes        JWT issuer
  Jwt\_\_Audience             Yes        JWT audience
  Jwt\_\_Secret               Yes        Minimum 64 random characters
  Jwt\_\_AccessTokenMinutes   No         Default 15
  Jwt\_\_RefreshTokenDays     No         Default 30

------------------------------------------------------------------------

# Google OAuth

  Variable                 Required
  ------------------------ ----------
  Google\_\_ClientId       Yes
  Google\_\_ClientSecret   Yes
  Google\_\_RedirectUri    Yes

------------------------------------------------------------------------

# Brevo

  Variable               Required
  ---------------------- ----------
  Brevo\_\_ApiKey        Yes
  Brevo\_\_SenderName    Yes
  Brevo\_\_SenderEmail   Yes
  Brevo\_\_BaseUrl       No

------------------------------------------------------------------------

# Cloudinary

  Variable                      Required
  ----------------------------- ----------
  Cloudinary\_\_CloudName       Yes
  Cloudinary\_\_ApiKey          Yes
  Cloudinary\_\_ApiSecret       Yes
  Cloudinary\_\_ProductFolder   No
  Cloudinary\_\_LogoFolder      No

Recommended folder structure:

-   tenantId/products
-   tenantId/logos
-   tenantId/banners

------------------------------------------------------------------------

# Razorpay (Platform)

  Variable                    Required
  --------------------------- ----------
  Razorpay\_\_KeyId           Yes
  Razorpay\_\_KeySecret       Yes
  Razorpay\_\_WebhookSecret   Yes

Tenant Razorpay credentials are stored encrypted in the database.

------------------------------------------------------------------------

# Logging

  Variable                        Default
  ------------------------------- -------------
  Serilog\_\_MinimumLevel         Information
  Serilog\_\_WriteTo\_\_Console   true
  Serilog\_\_WriteTo\_\_File      true

------------------------------------------------------------------------

# Swagger

  Variable             Default
  -------------------- ---------
  Swagger\_\_Enabled   true

Production recommendation: disable public Swagger unless protected.

------------------------------------------------------------------------

# CORS

  Variable                 Description
  ------------------------ -------------------------
  Cors\_\_AllowedOrigins   Comma-separated origins

------------------------------------------------------------------------

# Background Workers

  Variable                               Default
  -------------------------------------- ---------
  Workers\_\_EmailIntervalSeconds        10
  Workers\_\_RefundIntervalSeconds       30
  Workers\_\_StatisticsIntervalMinutes   10
  Workers\_\_CleanupIntervalHours        24

------------------------------------------------------------------------

# Outbox

  Variable               Default
  ---------------------- ---------
  Outbox\_\_BatchSize    50
  Outbox\_\_RetryCount   5

------------------------------------------------------------------------

# Render Notes

Render automatically provides:

-   PORT

Use:

``` csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");
```

Health endpoint:

-   GET /health
-   HEAD /health

------------------------------------------------------------------------

# Startup Validation

The application should fail startup if any required configuration is
missing for:

-   Database
-   JWT
-   Cloudinary
-   Brevo
-   Google OAuth
-   Application URLs

------------------------------------------------------------------------

# Local Development

Use:

-   User Secrets for secrets
-   appsettings.Development.json for non-sensitive configuration

Never commit:

-   API keys
-   Passwords
-   Connection strings
-   Secrets

------------------------------------------------------------------------

# Production Checklist

-   Environment variables configured
-   HTTPS enabled
-   HSTS enabled
-   Security headers enabled
-   Secrets never logged
-   Startup validation enabled

------------------------------------------------------------------------

# Next Document

**07-DockerAndDeployment.md**

Topics:

-   Multi-stage Dockerfile
-   Render deployment
-   EF migrations on startup
-   Health checks
-   Graceful shutdown
-   Local Docker Compose
