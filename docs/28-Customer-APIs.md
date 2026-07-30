# Kromic Store Backend Implementation Guide

# Phase 03 -- 28 Customer APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the APIs used by customers and tenant administrators to manage
customer accounts, profiles, addresses, wishlists, preferences, and
account history.

------------------------------------------------------------------------

# Authorization

  Role           Access
  -------------- ------------------------------------------
  Customer       Own profile only
  TenantAdmin    Full customer management
  StoreManager   Read/update customer data (configurable)

------------------------------------------------------------------------

# Endpoint Catalog

## Customer Profile

  Method   Endpoint                 Description
  -------- ------------------------ --------------------------
  GET      /api/v1/customers/me     Current customer profile
  PUT      /api/v1/customers/me     Update profile
  GET      /api/v1/customers/{id}   Admin view
  GET      /api/v1/customers        Customer search

------------------------------------------------------------------------

## Addresses

  Method   Endpoint
  -------- ---------------------------------------------
  GET      /api/v1/customers/me/addresses
  POST     /api/v1/customers/me/addresses
  PUT      /api/v1/customers/me/addresses/{id}
  DELETE   /api/v1/customers/me/addresses/{id}
  POST     /api/v1/customers/me/addresses/{id}/default

Rules:

-   Multiple addresses supported
-   One default billing address
-   One default shipping address

------------------------------------------------------------------------

## Wishlist

  Method   Endpoint
  -------- -------------------------------------------
  GET      /api/v1/customers/me/wishlist
  POST     /api/v1/customers/me/wishlist
  DELETE   /api/v1/customers/me/wishlist/{productId}

Rules:

-   Duplicate products not allowed.
-   Soft-delete unavailable products from view.

------------------------------------------------------------------------

## Preferences

Manage:

-   Preferred language
-   Preferred currency
-   Marketing emails
-   SMS notifications
-   Push notifications (future)

Endpoint:

-   PUT /api/v1/customers/me/preferences

------------------------------------------------------------------------

## Order History

  Method   Endpoint
  -------- ---------------------------------------
  GET      /api/v1/customers/me/orders
  GET      /api/v1/customers/me/orders/{orderId}

Customers can access only their own orders.

------------------------------------------------------------------------

## Customer Groups (Admin)

-   GET /api/v1/customer-groups
-   POST /api/v1/customer-groups
-   PUT /api/v1/customer-groups/{id}
-   DELETE /api/v1/customer-groups/{id}

Examples:

-   VIP
-   Wholesale
-   Retail

------------------------------------------------------------------------

## Internal Notes

Admin only:

-   GET /api/v1/customers/{id}/notes
-   POST /api/v1/customers/{id}/notes

------------------------------------------------------------------------

# Validation

-   Email format
-   Phone format
-   Address completeness
-   Duplicate wishlist prevention

------------------------------------------------------------------------

# Business Rules

-   Tenant isolation enforced.
-   Customers cannot modify other accounts.
-   Profile changes are audited.
-   Order history is immutable.

------------------------------------------------------------------------

# Future Enhancements

-   Loyalty points
-   Saved payment methods
-   Reward tiers
-   Gift registry
-   Recently viewed products

------------------------------------------------------------------------

# Testing

Verify:

-   Profile updates
-   Address CRUD
-   Default address switching
-   Wishlist operations
-   Preference updates
-   Customer group management
-   Order history authorization

------------------------------------------------------------------------

# Next Document

**29-Cart-and-Checkout-APIs.md**

Topics:

-   Cart management
-   Checkout session
-   Coupons
-   Shipping
-   Payment initiation
-   Order placement
