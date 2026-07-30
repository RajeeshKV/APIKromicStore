# Kromic Store Backend Implementation Guide

# Phase 03 -- 29 Cart and Checkout APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the complete shopping and checkout API surface, from adding
products to the cart through successful order placement.

------------------------------------------------------------------------

# Authorization

  Role          Access
  ------------- ----------------------------------
  Customer      Full
  Guest         Supported (limited)
  TenantAdmin   Read-only support tools (future)

------------------------------------------------------------------------

# Shopping Flow

``` text
Browse Products
      ↓
Add To Cart
      ↓
Update Cart
      ↓
Guest Login (optional)
      ↓
Merge Guest Cart
      ↓
Create Checkout Session
      ↓
Select Address
      ↓
Select Shipping
      ↓
Apply Coupon
      ↓
Review Order
      ↓
Initialize Payment
      ↓
Place Order
```

------------------------------------------------------------------------

# Cart APIs

  Method   Endpoint                  Description
  -------- ------------------------- -----------------
  GET      /api/v1/cart              Get active cart
  POST     /api/v1/cart/items        Add item
  PUT      /api/v1/cart/items/{id}   Update quantity
  DELETE   /api/v1/cart/items/{id}   Remove item
  DELETE   /api/v1/cart              Clear cart

Rules:

-   One active cart per customer
-   Guest carts identified by anonymous session
-   Automatic merge after authentication

------------------------------------------------------------------------

# Guest Cart Merge

Endpoint:

POST /api/v1/cart/merge

Behavior:

-   Merge duplicate items
-   Preserve highest quantity
-   Remove guest cart after merge
-   Return updated customer cart

------------------------------------------------------------------------

# Checkout Session APIs

  Method   Endpoint
  -------- -------------------------------
  POST     /api/v1/checkout/session
  GET      /api/v1/checkout/session/{id}
  PUT      /api/v1/checkout/session/{id}
  DELETE   /api/v1/checkout/session/{id}

Checkout contains:

-   Customer
-   Addresses
-   Shipping
-   Coupon
-   Payment
-   Totals

------------------------------------------------------------------------

# Shipping APIs

  Method   Endpoint
  -------- -----------------------------------
  GET      /api/v1/checkout/shipping-methods
  POST     /api/v1/checkout/shipping

Future:

-   Real-time shipping providers

------------------------------------------------------------------------

# Coupon APIs

  Method   Endpoint
  -------- --------------------------------
  POST     /api/v1/checkout/coupons/apply
  DELETE   /api/v1/checkout/coupons

Validation:

-   Expiry
-   Usage limits
-   Customer eligibility
-   Minimum purchase amount

------------------------------------------------------------------------

# Pricing

Checkout recalculates:

-   Product prices
-   Discounts
-   Shipping
-   Taxes
-   Grand total

Never trust client-side totals.

------------------------------------------------------------------------

# Payment Initialization

Endpoint:

POST /api/v1/checkout/payment

Supports:

-   Razorpay

Returns:

-   Provider order ID
-   Amount
-   Currency
-   Client payment configuration

------------------------------------------------------------------------

# Order Placement

Endpoint:

POST /api/v1/checkout/place-order

Business Rules:

-   Inventory revalidated
-   Payment verified
-   Order created atomically
-   Outbox event created
-   Cart cleared

------------------------------------------------------------------------

# Idempotency

Require the `Idempotency-Key` header for order placement and payment
initialization to prevent duplicate orders.

------------------------------------------------------------------------

# Validation

-   Valid address
-   Shipping method selected
-   Inventory available
-   Coupon valid
-   Payment initialized

------------------------------------------------------------------------

# Error Scenarios

  Scenario                Status
  --------------------- --------
  Empty cart                 400
  Product unavailable        409
  Coupon expired             400
  Inventory changed          409
  Payment failed             402
  Checkout expired           410

------------------------------------------------------------------------

# Testing

Verify:

-   Guest cart merge
-   Cart CRUD
-   Coupon validation
-   Shipping selection
-   Payment initialization
-   Order placement
-   Idempotency
-   Inventory conflicts

------------------------------------------------------------------------

# Next Document

**30-Order-and-Payment-APIs.md**

Topics:

-   Order lifecycle
-   Payment callbacks
-   Refunds
-   Shipment tracking
-   Invoice download
-   Order cancellation
