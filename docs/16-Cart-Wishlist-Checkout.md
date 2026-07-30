# Kromic Store Backend Implementation Guide

# Phase 02 -- 16 Cart, Wishlist & Checkout

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the complete shopping journey from adding products to the cart
through successful order creation.

Goals:

-   Persistent carts
-   Fast checkout
-   Tenant isolation
-   Inventory consistency
-   Extensible checkout pipeline

------------------------------------------------------------------------

# Entity Overview

``` text
Cart
 ├── CartItem
Wishlist
 ├── WishlistItem
CheckoutSession
 ├── CheckoutCoupon
 ├── CheckoutShipping
 └── CheckoutPayment
```

------------------------------------------------------------------------

# Cart

Represents an active shopping cart.

Columns:

-   Id
-   TenantId
-   CustomerId (nullable)
-   AnonymousSessionId (nullable)
-   Currency
-   LastActivityOnUtc
-   ExpiresOnUtc

Rules:

-   One active cart per customer.
-   Guest carts supported.
-   Guest cart merges after login.

------------------------------------------------------------------------

# CartItem

Columns:

-   CartId
-   ProductId
-   ProductVariantId (nullable)
-   Quantity
-   UnitPrice
-   AddedOnUtc

Rules:

-   Quantity must be positive.
-   Product availability validated before checkout.

------------------------------------------------------------------------

# Wishlist

Stores a customer's wishlist.

Columns:

-   Id
-   TenantId
-   CustomerId

One wishlist per customer.

------------------------------------------------------------------------

# WishlistItem

Columns:

-   WishlistId
-   ProductId
-   AddedOnUtc

Duplicate products are not allowed.

------------------------------------------------------------------------

# CheckoutSession

Represents an in-progress checkout.

Columns:

-   Id
-   TenantId
-   CustomerId
-   BillingAddressId
-   ShippingAddressId
-   ShippingMethod
-   PaymentMethod
-   Status
-   CreatedOnUtc
-   ExpiresOnUtc

Statuses:

-   Draft
-   AwaitingPayment
-   Completed
-   Expired
-   Cancelled

------------------------------------------------------------------------

# CheckoutCoupon

Columns:

-   CheckoutSessionId
-   CouponCode
-   DiscountAmount
-   DiscountType

Coupons are validated before order creation.

------------------------------------------------------------------------

# CheckoutShipping

Columns:

-   CheckoutSessionId
-   Carrier
-   Service
-   TrackingReference (nullable)
-   ShippingCost
-   EstimatedDelivery

------------------------------------------------------------------------

# CheckoutPayment

Columns:

-   CheckoutSessionId
-   Provider
-   PaymentReference
-   Amount
-   Status

Supports Razorpay initially.

------------------------------------------------------------------------

# Checkout Flow

``` text
Add To Cart
    ↓
Update Quantity
    ↓
Login (optional)
    ↓
Merge Guest Cart
    ↓
Select Address
    ↓
Select Shipping
    ↓
Apply Coupon
    ↓
Initiate Payment
    ↓
Create Order
```

------------------------------------------------------------------------

# Inventory Rules

-   Validate inventory before payment.
-   Reserve inventory during checkout.
-   Release reservation on expiry or payment failure.
-   Deduct inventory only after successful order creation.

------------------------------------------------------------------------

# Business Rules

-   Prices are recalculated during checkout.
-   Cart items do not guarantee inventory.
-   Checkout sessions expire automatically.
-   Wishlist does not reserve inventory.

------------------------------------------------------------------------

# Recommended Indexes

-   (TenantId, CustomerId)
-   (CartId, ProductId)
-   (WishlistId, ProductId)
-   (CheckoutSessionId, Status)

------------------------------------------------------------------------

# Background Jobs

-   Expire abandoned carts
-   Expire checkout sessions
-   Release reserved inventory
-   Cleanup stale guest carts

------------------------------------------------------------------------

# Testing

Verify:

-   Guest cart merge
-   Inventory validation
-   Coupon validation
-   Checkout expiry
-   Wishlist duplicates
-   Payment initiation
-   Order creation

------------------------------------------------------------------------

# Future Enhancements

-   Saved carts
-   Buy Now flow
-   Gift wrapping
-   Multiple shipping addresses
-   Store pickup
-   Scheduled delivery

------------------------------------------------------------------------

# Next Document

**17-Orders-And-Payments.md**

Topics:

-   Orders
-   Order items
-   Order status lifecycle
-   Payments
-   Refunds
-   Shipment tracking
-   Order history
