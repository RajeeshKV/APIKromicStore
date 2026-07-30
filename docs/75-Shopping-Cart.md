# Kromic Store Frontend Documentation

# Phase 05 -- 75 Shopping Cart

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

The Shopping Cart provides customers with a clear summary of their
selected items before checkout. It should make reviewing, updating, and
completing purchases simple while encouraging higher order values
through recommendations and promotions.

------------------------------------------------------------------------

# Goals

-   Reduce cart abandonment
-   Simplify cart management
-   Increase average order value
-   Support seamless checkout
-   Preserve cart state across sessions

------------------------------------------------------------------------

# Module Overview

The Shopping Cart experience includes:

-   Mini Cart (Cart Drawer)
-   Full Cart Page
-   Cart Summary
-   Coupons & Promotions
-   Shipping Estimation
-   Saved for Later
-   Recommendations
-   Cart Persistence

------------------------------------------------------------------------

# Mini Cart (Drawer)

The cart drawer opens from any page without navigation.

Display:

-   Product image
-   Product name
-   Selected variants
-   Quantity
-   Item price
-   Remove action
-   Cart subtotal

Actions:

-   View Cart
-   Continue Shopping
-   Checkout

------------------------------------------------------------------------

# Full Cart Page

Display:

-   Cart items
-   Product image
-   Product details
-   Variant selection
-   Quantity selector
-   Unit price
-   Line total
-   Remove item
-   Save for later

------------------------------------------------------------------------

# Quantity Management

Support:

-   Increase quantity
-   Decrease quantity
-   Manual quantity entry
-   Stock validation
-   Maximum purchase limits

Update totals instantly.

------------------------------------------------------------------------

# Cart Summary

Display:

-   Subtotal
-   Discounts
-   Shipping estimate
-   Taxes
-   Grand Total

Update dynamically whenever cart contents change.

------------------------------------------------------------------------

# Coupons & Promotions

Support:

-   Coupon codes
-   Automatic discounts
-   Gift cards
-   Promotional banners

Display validation messages clearly.

------------------------------------------------------------------------

# Shipping Estimation

Allow customers to estimate shipping using:

-   Postal code
-   City (future)
-   Country

Display:

-   Available methods
-   Estimated delivery
-   Shipping cost

------------------------------------------------------------------------

# Saved for Later

Allow customers to:

-   Save items
-   Restore items
-   Move between cart and saved list

Persist saved items across sessions.

------------------------------------------------------------------------

# Product Recommendations

Display:

-   Frequently Bought Together
-   Customers Also Bought
-   Trending Products
-   Recently Viewed

Recommendations should never interrupt checkout flow.

------------------------------------------------------------------------

# Cart Persistence

Persist cart across:

-   Page refreshes
-   Devices (authenticated users)
-   Browser sessions

Synchronize changes after sign-in.

------------------------------------------------------------------------

# Empty Cart

Display:

-   Friendly illustration
-   Continue Shopping button
-   Featured products
-   Popular categories

Encourage customers to resume browsing.

------------------------------------------------------------------------

# Notifications

Show notifications for:

-   Item added
-   Item removed
-   Quantity updated
-   Coupon applied
-   Inventory changes

------------------------------------------------------------------------

# Performance

Optimize with:

-   Instant UI updates
-   Lazy-loaded recommendations
-   Optimistic quantity updates
-   Cached cart state

------------------------------------------------------------------------

# Responsive Design

Desktop:

-   Side-by-side cart and summary

Tablet:

-   Condensed layout

Mobile:

-   Stacked sections
-   Sticky checkout button
-   Bottom-sheet coupon entry (optional)

------------------------------------------------------------------------

# Accessibility

Ensure:

-   Keyboard navigation
-   Accessible quantity controls
-   Screen-reader announcements
-   High-contrast buttons
-   Proper focus management

------------------------------------------------------------------------

# Best Practices

-   Keep checkout easily accessible.
-   Show transparent pricing.
-   Avoid unnecessary interruptions.
-   Preserve customer selections.
-   Highlight savings without distraction.

------------------------------------------------------------------------

# Next Document

**76 -- Checkout**

Topics:

-   Customer information
-   Shipping
-   Billing
-   Payment
-   Order review
-   Confirmation
-   Guest checkout
-   Validation
