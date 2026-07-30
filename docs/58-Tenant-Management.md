# Kromic Store Frontend Documentation

# Phase 04 -- 58 Tenant Management

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the Tenant Management experience for Super Admins. This module
enables platform administrators to create, manage, monitor, and support
tenants throughout their lifecycle.

------------------------------------------------------------------------

# Goals

-   Efficient tenant administration
-   Complete tenant visibility
-   Subscription management
-   Operational support
-   Platform governance

------------------------------------------------------------------------

# Module Overview

The Tenant Management module consists of:

-   Tenant List
-   Tenant Details
-   Create Tenant
-   Edit Tenant
-   Subscription Management
-   Domain Management
-   Branding Overview
-   Audit History

------------------------------------------------------------------------

# Tenant List

Display tenants in a searchable, filterable data grid.

Columns:

-   Logo
-   Store Name
-   Tenant ID
-   Owner
-   Plan
-   Status
-   Domains
-   Created Date
-   Last Activity
-   Actions

Support:

-   Sorting
-   Pagination
-   Column customization
-   Bulk selection

------------------------------------------------------------------------

# Search & Filters

Search by:

-   Store name
-   Owner
-   Email
-   Domain
-   Tenant ID

Filters:

-   Status
-   Plan
-   Trial
-   Subscription expiry
-   Created date
-   Last activity
-   Region

------------------------------------------------------------------------

# Tenant Details

Sections:

## Overview

-   Logo
-   Store name
-   Owner
-   Contact information
-   Tenant ID
-   Status

## Subscription

-   Current plan
-   Billing cycle
-   Renewal date
-   Usage limits

## Domains

-   Primary domain
-   Custom domains
-   Verification status

## Branding

-   Logo
-   Colors
-   Active theme

## Usage

-   Products
-   Orders
-   Customers
-   Storage
-   API usage

------------------------------------------------------------------------

# Create Tenant Flow

Steps:

1.  Basic information
2.  Owner account
3.  Subscription plan
4.  Domain selection
5.  Branding
6.  Review
7.  Provision tenant

Automatically trigger initial onboarding after creation.

------------------------------------------------------------------------

# Edit Tenant

Allow updates to:

-   Contact information
-   Branding
-   Subscription
-   Domains
-   Feature flags
-   Status

Maintain a full audit trail.

------------------------------------------------------------------------

# Tenant Lifecycle

Supported states:

-   Draft
-   Trial
-   Active
-   Suspended
-   Expired
-   Archived

Display clear visual indicators for each state.

------------------------------------------------------------------------

# Subscription Management

Support:

-   Upgrade
-   Downgrade
-   Renew
-   Cancel
-   Trial extension

Display plan features and usage limits.

------------------------------------------------------------------------

# Domain Management

Capabilities:

-   Add domain
-   Remove domain
-   Verify domain
-   Set primary domain
-   SSL status

Highlight verification issues.

------------------------------------------------------------------------

# Bulk Actions

Allow:

-   Activate
-   Suspend
-   Archive
-   Assign plan
-   Export
-   Delete (where permitted)

Require confirmation for destructive actions.

------------------------------------------------------------------------

# Audit History

Track:

-   Tenant creation
-   Updates
-   Plan changes
-   Domain changes
-   Status changes
-   Login history

Provide filtering and export.

------------------------------------------------------------------------

# Quick Actions

-   View Store
-   Open Dashboard
-   Impersonate
-   Manage Subscription
-   Reset Owner Password
-   Contact Owner

------------------------------------------------------------------------

# Loading & Empty States

Support:

-   Skeleton tables
-   Empty tenant list
-   Empty search results
-   Retry actions
-   Progressive loading

------------------------------------------------------------------------

# Accessibility

Ensure:

-   Keyboard-accessible tables
-   Focus management
-   Screen-reader labels
-   Accessible filter controls

------------------------------------------------------------------------

# Best Practices

-   Surface important tenant information first.
-   Minimize clicks for common actions.
-   Prevent accidental destructive operations.
-   Keep audit history immutable.
-   Support future scalability for thousands of tenants.

------------------------------------------------------------------------

# Next Document

**59-Platform-Settings.md**

Topics:

-   Global settings
-   Branding
-   Email configuration
-   Feature flags
-   Integrations
-   Maintenance mode
-   Security settings
-   Platform preferences
