# Kromic Store Backend Implementation Guide

# Phase 02 -- 15 Customer Database

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the customer data model used by every tenant
storefront.

Objectives:

-   Tenant-isolated customers
-   Multiple saved addresses
-   Marketing preferences
-   Customer insights
-   Future extensibility

------------------------------------------------------------------------

# Entity Overview

``` text
Customer
 ├── CustomerAddress
 ├── CustomerPreference
 ├── CustomerGroup
 ├── CustomerGroupMember
 ├── CustomerNote
 └── CustomerStatistics
```

------------------------------------------------------------------------

# Customer

Stores the customer's core profile.

Columns:

-   Id
-   TenantId
-   Email
-   FirstName
-   LastName
-   PhoneNumber
-   DateOfBirth (optional)
-   Gender (optional)
-   Status
-   LastLoginOnUtc

Inherited:

-   Audit fields
-   Soft delete fields

Rules:

-   Email is unique per tenant.
-   Same email may exist in different tenants.
-   Customer belongs to exactly one tenant.

Indexes:

-   UX_Customer_Tenant_Email
-   IX_Customer_Status

------------------------------------------------------------------------

# CustomerAddress

Stores multiple addresses.

Columns:

-   Id
-   CustomerId
-   AddressType (Billing / Shipping / Both)
-   RecipientName
-   PhoneNumber
-   AddressLine1
-   AddressLine2
-   Landmark
-   City
-   State
-   Country
-   PostalCode
-   IsDefaultBilling
-   IsDefaultShipping

Rules:

-   Multiple addresses supported.
-   Only one default billing and shipping address.

------------------------------------------------------------------------

# CustomerPreference

Stores communication preferences.

Columns:

-   CustomerId
-   EmailMarketing
-   SmsMarketing
-   WhatsAppMarketing
-   PreferredLanguage
-   PreferredCurrency

------------------------------------------------------------------------

# CustomerGroup

Examples:

-   Retail
-   Wholesale
-   VIP
-   Employee

Columns:

-   Id
-   TenantId
-   Name
-   Description

------------------------------------------------------------------------

# CustomerGroupMember

Many-to-many relationship.

Columns:

-   CustomerId
-   CustomerGroupId

------------------------------------------------------------------------

# CustomerNote

Private notes visible only to tenant staff.

Columns:

-   Id
-   CustomerId
-   Note
-   CreatedBy
-   CreatedOnUtc

Examples:

-   Preferred delivery window
-   Manual follow-up required
-   VIP handling

------------------------------------------------------------------------

# CustomerStatistics

Cached summary for dashboard performance.

Columns:

-   CustomerId
-   TotalOrders
-   TotalSpent
-   AverageOrderValue
-   LastOrderOnUtc
-   LifetimeValue

Updated asynchronously after completed orders.

------------------------------------------------------------------------

# Business Rules

-   Customers cannot belong to multiple tenants.
-   Address ownership is enforced.
-   Notes are never visible to customers.
-   Statistics are derived, not user editable.
-   Soft delete applies to all entities.

------------------------------------------------------------------------

# Recommended Indexes

-   (TenantId, Email)
-   (CustomerId, IsDefaultShipping)
-   (CustomerId, IsDefaultBilling)
-   (TenantId, Status)

------------------------------------------------------------------------

# Testing

Verify:

-   Tenant isolation
-   Email uniqueness
-   Default address validation
-   Group assignment
-   Preference persistence
-   Statistics updates

------------------------------------------------------------------------

# Future Enhancements

-   Loyalty points
-   Referral program
-   Store credit
-   Customer wallets
-   Saved payment methods
-   Membership tiers

------------------------------------------------------------------------

# Next Document

**16-Cart-Wishlist-Checkout.md**

Topics:

-   Shopping cart
-   Wishlist
-   Checkout session
-   Coupons
-   Shipping methods
-   Tax calculation
-   Order creation workflow
