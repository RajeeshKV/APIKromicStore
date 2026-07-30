# Kromic Store Backend Implementation Guide

# Phase 03 -- 35 CQRS Command Catalog

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the complete write-side architecture of Kromic Store using the
CQRS pattern.

Goals:

-   Consistent command design
-   Thin controllers
-   Single-responsibility handlers
-   Transactional consistency
-   Centralized validation and authorization

------------------------------------------------------------------------

# Folder Structure

``` text
Application/
├── Features/
│   ├── Identity/
│   ├── Tenant/
│   ├── Themes/
│   ├── Catalog/
│   ├── Customers/
│   ├── Cart/
│   ├── Checkout/
│   ├── Orders/
│   ├── Dashboard/
│   └── Admin/
```

Each feature contains:

-   Commands/
-   Handlers/
-   Validators/
-   DTOs/

------------------------------------------------------------------------

# Naming Conventions

-   CreateProductCommand
-   UpdateProductCommand
-   DeleteProductCommand
-   PublishThemeCommand
-   PlaceOrderCommand

Handlers end with `CommandHandler`.

------------------------------------------------------------------------

# Command Pipeline

``` text
Controller
   ↓
Authorization
   ↓
Validation
   ↓
Transaction
   ↓
Command Handler
   ↓
Domain Events
   ↓
Outbox
```

------------------------------------------------------------------------

# Handler Responsibilities

Handlers should:

-   Execute one business use case
-   Persist changes
-   Publish domain events
-   Never return EF entities
-   Never contain presentation logic

------------------------------------------------------------------------

# Validation

Use FluentValidation.

Validate:

-   Input
-   Business invariants
-   Permissions (when applicable)

------------------------------------------------------------------------

# Transaction Rules

Use explicit transactions for:

-   Checkout
-   Payments
-   Orders
-   Inventory
-   Multi-entity updates

Read-only commands should not exist.

------------------------------------------------------------------------

# Command Catalog

## Identity

-   RegisterUserCommand
-   LoginUserCommand
-   RefreshTokenCommand
-   LogoutCommand
-   VerifyEmailCommand
-   ResetPasswordCommand

## Tenant

-   CreateTenantCommand
-   UpdateTenantCommand
-   UpdateBrandingCommand
-   UpdateSettingsCommand
-   AddDomainCommand
-   VerifyDomainCommand

## Themes

-   CreateThemeCommand
-   CloneThemeCommand
-   PublishThemeCommand
-   AssignThemeCommand
-   AddSectionCommand
-   ReorderSectionsCommand

## Catalog

-   CreateCategoryCommand
-   UpdateCategoryCommand
-   CreateProductCommand
-   UpdateProductCommand
-   DuplicateProductCommand
-   AdjustInventoryCommand
-   UploadProductImageCommand

## Customers

-   UpdateCustomerProfileCommand
-   AddAddressCommand
-   SetDefaultAddressCommand
-   UpdatePreferencesCommand

## Cart & Checkout

-   AddCartItemCommand
-   UpdateCartItemCommand
-   MergeGuestCartCommand
-   ApplyCouponCommand
-   CreateCheckoutSessionCommand
-   InitializePaymentCommand
-   PlaceOrderCommand

## Orders

-   ConfirmOrderCommand
-   ShipOrderCommand
-   DeliverOrderCommand
-   CancelOrderCommand
-   RefundOrderCommand

## Super Admin

-   SuspendTenantCommand
-   ActivateTenantCommand
-   UpdatePlatformSettingsCommand
-   ApproveThemeCommand
-   UpdateFeatureFlagsCommand

------------------------------------------------------------------------

# Domain Events

Examples:

-   ProductCreated
-   OrderPlaced
-   PaymentVerified
-   ThemePublished
-   TenantCreated

Events are persisted using the Outbox pattern.

------------------------------------------------------------------------

# Error Handling

Translate exceptions into API responses.

Never expose stack traces.

------------------------------------------------------------------------

# Testing

Verify:

-   Validation
-   Authorization
-   Transactions
-   Domain events
-   Outbox creation
-   Idempotency

------------------------------------------------------------------------

# Next Document

**36-CQRS-Query-Catalog.md**

Topics:

-   Query organization
-   Query handlers
-   Read models
-   Pagination
-   Projections
-   Caching
